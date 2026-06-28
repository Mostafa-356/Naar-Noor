#!/bin/bash

# Coverage Threshold Validation Script for NaarNoor Backend Tests (Linux/Mac)
# Purpose: Validates per-layer code coverage against defined thresholds and reports detailed gaps
# Reference: Requirement 1.1 - Backend coverage must be measured with layer-specific thresholds
#
# Thresholds:
#   - Domain: 85% line coverage
#   - Application: 82% line coverage  
#   - Infrastructure: 78% line coverage
#   - API: 80% line coverage
#
# Features:
#   - Per-layer threshold checking
#   - Detailed coverage gap reporting
#   - Coverage file analysis and validation
#   - Uncovered code identification
#   - Exit codes for CI/CD integration
#
# Usage:
#   ./scripts/validate-coverage.sh [--test-dir <dir>] [--report-dir <dir>]
#
# Examples:
#   ./scripts/validate-coverage.sh
#   ./scripts/validate-coverage.sh --test-dir ./api-server/tests --report-dir ./coverage

set -euo pipefail

# Parse command line arguments
TEST_RESULTS_DIR="${TEST_RESULTS_DIR:-api-server/tests}"
REPORT_OUTPUT_DIR="${REPORT_OUTPUT_DIR:-coverage}"

while [[ $# -gt 0 ]]; do
    case $1 in
        --test-dir)
            TEST_RESULTS_DIR="$2"
            shift 2
            ;;
        --report-dir)
            REPORT_OUTPUT_DIR="$2"
            shift 2
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

# Define per-layer thresholds (Requirement 1.1)
declare -A LAYER_THRESHOLDS=(
    ["NaarNoor.Domain"]=85
    ["NaarNoor.Application"]=82
    ["NaarNoor.Infrastructure"]=78
    ["NaarNoor.API"]=80
)

# Initialize tracking
ALL_LAYERS_PASS=true
declare -a LAYER_RESULTS
declare -a GAP_DETAILS

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
MAGENTA='\033[0;35m'
NC='\033[0m' # No Color

# Function to extract line coverage percentage from Cobertura XML
get_line_coverage_from_xml() {
    local xml_path="$1"
    
    if [[ ! -f "$xml_path" ]]; then
        echo ""
        return
    fi
    
    # Extract line-rate attribute and convert to percentage
    local line_rate=$(grep -oP 'coverage[^>]*?line-rate="\K[^"]+' "$xml_path" | head -1)
    
    if [[ -n "$line_rate" ]]; then
        # Convert decimal (0.85) to percentage (85.00)
        local percentage=$(awk "BEGIN {printf \"%.2f\", $line_rate * 100}")
        echo "$percentage"
    fi
}

# Function to extract coverage for specific package
get_package_coverage() {
    local xml_path="$1"
    local package_name="$2"
    
    if [[ ! -f "$xml_path" ]]; then
        echo ""
        return
    fi
    
    # Extract package line-rate for matching package name
    local package_coverage=$(grep -A1 "package name=\"$package_name\"" "$xml_path" 2>/dev/null | grep -oP 'line-rate="\K[^"]+' | head -1)
    
    if [[ -z "$package_coverage" ]]; then
        # Fallback to overall coverage if package not found
        get_line_coverage_from_xml "$xml_path"
    else
        # Convert decimal to percentage
        local percentage=$(awk "BEGIN {printf \"%.2f\", $package_coverage * 100}")
        echo "$percentage"
    fi
}

# Function to identify uncovered classes in a package
get_uncovered_classes() {
    local xml_path="$1"
    local package_name="$2"
    local min_threshold="${3:-50}"
    
    if [[ ! -f "$xml_path" ]]; then
        return
    fi
    
    # Extract classes with low coverage (simplified extraction)
    # This is a best-effort approach using grep/awk for portability
    local in_package=false
    local class_count=0
    local low_coverage_classes=()
    
    while IFS= read -r line; do
        if [[ $line == *"package name=\"$package_name\"" ]]; then
            in_package=true
        elif [[ $in_package == true && $line == *"</package>" ]]; then
            in_package=false
        elif [[ $in_package == true && $line == *'<class'* ]]; then
            # Extract class name and line-rate
            local class_name=$(echo "$line" | grep -oP 'name="\K[^"]+' | head -1)
            local coverage=$(echo "$line" | grep -oP 'line-rate="\K[^"]+')
            
            if [[ -n "$coverage" ]]; then
                local coverage_percent=$(awk "BEGIN {printf \"%.2f\", $coverage * 100}")
                
                # Check if coverage is below threshold
                if (( $(echo "$coverage_percent < $min_threshold" | bc -l) )); then
                    low_coverage_classes+=("$class_name|$coverage_percent")
                    ((class_count++))
                fi
            fi
        fi
    done < "$xml_path"
    
    # Output first 5 low coverage classes
    local count=0
    for class_entry in "${low_coverage_classes[@]}"; do
        if [[ $count -lt 5 ]]; then
            IFS='|' read -r class_name class_coverage <<< "$class_entry"
            echo "    • $class_name: $class_coverage%"
            ((count++))
        fi
    done
    
    if [[ $class_count -gt 5 ]]; then
        echo "    • ... and $((class_count - 5)) more classes with < 50% coverage"
    fi
}

# Function to generate detailed coverage gap report
generate_coverage_gap_report() {
    local xml_path="$1"
    local package_name="$2"
    local threshold="$3"
    local current_coverage="$4"
    
    local gap=$(awk "BEGIN {printf \"%.2f\", $threshold - $current_coverage}")
    
    local report="────────────────────────────────────────────────────────────
Layer: $package_name
  Required Coverage:  $threshold%
  Actual Coverage:    $current_coverage%
  Coverage Gap:       $gap% below threshold
  
  Lowest Coverage Classes (< 50%):
"
    
    # Get uncovered classes
    local uncovered=$(get_uncovered_classes "$xml_path" "$package_name" "50")
    if [[ -n "$uncovered" ]]; then
        report+="$uncovered"$'\n'
    else
        report+="    (No classes with < 50% coverage found)"$'\n'
    fi
    
    report+="
  Recommended Actions:
  • Prioritize testing for low-coverage classes
  • Review design for testability issues
  • Add unit tests for uncovered code paths
────────────────────────────────────────────────────────────"
    
    echo "$report"
}

# Print header
echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${CYAN}   Code Coverage Validation Report - Per-Layer Analysis${NC}"
echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo ""

# Collect coverage reports from each test project
declare -a TEST_PROJECTS=(
    "Domain:NaarNoor.Domain.Tests:NaarNoor.Domain"
    "Application:NaarNoor.Application.Tests:NaarNoor.Application"
    "Infrastructure:NaarNoor.Infrastructure.Tests:NaarNoor.Infrastructure"
    "API:NaarNoor.API.Tests:NaarNoor.API"
)

for project_info in "${TEST_PROJECTS[@]}"; do
    IFS=':' read -r project_name test_dir package_name <<< "$project_info"
    
    local test_path="$TEST_RESULTS_DIR/$test_dir"
    
    # Find the coverage.cobertura.xml file
    local coverage_file=$(find "$test_path" -name "coverage.cobertura.xml" -type f 2>/dev/null | head -1)
    
    if [[ -z "$coverage_file" ]]; then
        echo -e "${YELLOW}⚠️  [$project_name] No coverage report found at $test_path${NC}"
        ALL_LAYERS_PASS=false
        continue
    fi
    
    # Extract coverage percentage
    local coverage_percent=$(get_package_coverage "$coverage_file" "$package_name")
    
    if [[ -z "$coverage_percent" ]]; then
        echo -e "${YELLOW}⚠️  [$project_name] Could not parse coverage from $coverage_file${NC}"
        ALL_LAYERS_PASS=false
        continue
    fi
    
    # Compare against threshold
    local threshold=${LAYER_THRESHOLDS["$package_name"]}
    local passed=true
    
    if (( $(echo "$coverage_percent < $threshold" | bc -l) )); then
        passed=false
    fi
    
    # Output status
    if [[ "$passed" == true ]]; then
        echo -e "${GREEN}✅ [$project_name] Coverage: $coverage_percent% (Threshold: $threshold%)${NC}"
    else
        local gap=$(awk "BEGIN {printf \"%.2f\", $threshold - $coverage_percent}")
        echo -e "${RED}❌ [$project_name] Coverage: $coverage_percent% (Threshold: $threshold%)${NC}"
        echo -e "${RED}   Gap: $gap% below threshold${NC}"
        ALL_LAYERS_PASS=false
        
        # Generate detailed gap report
        local detailed_report=$(generate_coverage_gap_report "$coverage_file" "$package_name" "$threshold" "$coverage_percent")
        GAP_DETAILS+=("$detailed_report")
    fi
    
    # Store result
    LAYER_RESULTS+=("$project_name:$coverage_percent:$threshold:$passed:$coverage_file")
done

echo ""
echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"

# Output detailed gap report if validation failed
if [[ "$ALL_LAYERS_PASS" == false ]] && [[ ${#GAP_DETAILS[@]} -gt 0 ]]; then
    echo ""
    echo -e "${MAGENTA}COVERAGE GAP ANALYSIS${NC}"
    echo -e "${MAGENTA}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo ""
    
    for detail in "${GAP_DETAILS[@]}"; do
        echo -e "${YELLOW}$detail${NC}"
    done
    
    # Save gap report to file for CI/CD
    mkdir -p "$REPORT_OUTPUT_DIR"
    local gap_report_path="$REPORT_OUTPUT_DIR/coverage-gap-report.txt"
    
    {
        printf '%s\n\n' "${GAP_DETAILS[@]}"
    } > "$gap_report_path"
    
    echo ""
    echo -e "${YELLOW}📄 Detailed gap report saved to: $gap_report_path${NC}"
    echo ""
fi

echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo ""

# Output results
if [[ "$ALL_LAYERS_PASS" == true ]]; then
    echo -e "${GREEN}✅ ALL LAYERS PASSED${NC}"
    echo -e "${GREEN}Coverage thresholds met for all layers.${NC}"
    echo ""
    
    echo -e "${GREEN}Summary:${NC}"
    printf "%-20s %12s %12s %10s\n" "Layer" "Coverage" "Threshold" "Status"
    printf "%-20s %12s %12s %10s\n" "────────────────────" "────────────" "────────────" "──────────"
    
    for result in "${LAYER_RESULTS[@]}"; do
        IFS=':' read -r layer coverage threshold passed <<< "$result"
        local status="✅ PASS"
        printf "%-20s %12s %12s %10s\n" "$layer" "$coverage%" "$threshold%" "$status"
    done
    
    echo ""
    exit 0
else
    echo -e "${RED}❌ COVERAGE VALIDATION FAILED${NC}"
    echo -e "${RED}One or more layers failed coverage thresholds.${NC}"
    echo ""
    
    echo -e "${RED}Summary:${NC}"
    printf "%-20s %12s %12s %10s\n" "Layer" "Coverage" "Threshold" "Status"
    printf "%-20s %12s %12s %10s\n" "────────────────────" "────────────" "────────────" "──────────"
    
    for result in "${LAYER_RESULTS[@]}"; do
        IFS=':' read -r layer coverage threshold passed <<< "$result"
        
        if [[ "$passed" == "true" ]]; then
            printf "%-20s %12s %12s %10s\n" "$layer" "$coverage%" "$threshold%" "✅ PASS"
        else
            printf "%-20s %12s %12s %10s\n" "$layer" "$coverage%" "$threshold%" "❌ FAIL"
        fi
    done
    
    echo ""
    echo -e "${YELLOW}Next Steps:${NC}"
    echo "  1. Review the coverage gap report above"
    echo "  2. Add tests for low-coverage classes"
    echo "  3. Re-run tests with: dotnet test --collect:\"XPlat Code Coverage\""
    echo ""
    
    exit 1
fi
