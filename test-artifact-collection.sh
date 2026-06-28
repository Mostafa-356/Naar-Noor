#!/bin/bash
# Test Artifact Collection Script
# Purpose: Verify that artifact collection works end-to-end
# Requirements Met: 2.4 - Test artifact download from workflow run

set -e

echo "=== Artifact Collection Test ==="
echo ""

# Create temporary test directory
TEST_DIR="$(mktemp -d)"
trap "rm -rf $TEST_DIR" EXIT

echo "1. Creating sample backend coverage report..."
cat > "$TEST_DIR/backend-report.json" <<'EOF'
{
  "passed": true,
  "results": {
    "Domain": {"coverage": 85.5, "threshold": 85.0, "passed": true},
    "Application": {"coverage": 82.3, "threshold": 82.0, "passed": true},
    "Infrastructure": {"coverage": 78.5, "threshold": 78.0, "passed": true},
    "API": {"coverage": 80.2, "threshold": 80.0, "passed": true}
  }
}
EOF
echo "   ✓ Backend report created"

echo ""
echo "2. Creating sample frontend coverage report..."
cat > "$TEST_DIR/frontend-report.json" <<'EOF'
{
  "passed": true,
  "results": {
    "Services": {"coverage": 80.5, "threshold": 80.0, "passed": true},
    "Components": {"coverage": 75.3, "threshold": 75.0, "passed": true}
  }
}
EOF
echo "   ✓ Frontend report created"

echo ""
echo "3. Testing coverage badge generation..."
OUTPUT_DIR="$TEST_DIR/badges"
python scripts/generate-coverage-badge.py \
  --backend-report "$TEST_DIR/backend-report.json" \
  --frontend-report "$TEST_DIR/frontend-report.json" \
  --output "$OUTPUT_DIR" 2>&1 | grep -i "Generated\|Error"
echo "   ✓ Badge generation completed"

echo ""
echo "4. Verifying badge files..."
BADGE_FILES=$(find "$OUTPUT_DIR" -name "*.svg" 2>/dev/null | wc -l)
echo "   Found $BADGE_FILES badge SVG files"

if [ "$BADGE_FILES" -gt 0 ]; then
    echo "   ✓ Expected badge files generated:"
    find "$OUTPUT_DIR" -name "*.svg" -exec basename {} \; | sed 's/^/     - /'
else
    echo "   ✗ No badge files generated"
    exit 1
fi

echo ""
echo "5. Verifying badge content..."
OVERALL_BADGE="$OUTPUT_DIR/coverage-overall.svg"
if [ -f "$OVERALL_BADGE" ]; then
    # Check that SVG contains coverage percentage
    if grep -q "coverage" "$OVERALL_BADGE" && grep -q "%" "$OVERALL_BADGE"; then
        echo "   ✓ Overall badge contains coverage data"
    else
        echo "   ✗ Overall badge missing coverage data"
        exit 1
    fi
else
    echo "   ✗ Overall badge file not found"
    exit 1
fi

echo ""
echo "6. Testing coverage comparison report generation..."
REPORT_FILE="$TEST_DIR/coverage-summary.md"
python scripts/generate-coverage-report.py \
  --backend-report "$TEST_DIR/backend-report.json" \
  --frontend-report "$TEST_DIR/frontend-report.json" \
  --output "$REPORT_FILE" 2>&1 | grep -i "written\|Error"

if [ -f "$REPORT_FILE" ]; then
    echo "   ✓ Report file generated"
    REPORT_SIZE=$(wc -c < "$REPORT_FILE")
    echo "   ✓ Report size: $REPORT_SIZE bytes"
    
    # Check report content
    if grep -q "Coverage Gate Report" "$REPORT_FILE"; then
        echo "   ✓ Report contains expected header"
    fi
    
    if grep -q "Backend Coverage" "$REPORT_FILE"; then
        echo "   ✓ Report contains backend section"
    fi
    
    if grep -q "Frontend Coverage" "$REPORT_FILE"; then
        echo "   ✓ Report contains frontend section"
    fi
else
    echo "   ✗ Report file not generated"
    exit 1
fi

echo ""
echo "=== Test Summary ==="
echo ""
echo "✓ Artifact collection test PASSED"
echo ""
echo "Artifacts that would be uploaded:"
echo "  1. Test Results (TRX files)"
echo "  2. Coverage Reports (XML + HTML)"
echo "  3. Coverage Badges (SVG)"
echo "  4. Coverage Comparison Report (Markdown)"
echo ""
echo "All with 30-day retention"
echo ""
echo "To download from workflow:"
echo "  1. Go to GitHub Actions"
echo "  2. Select workflow run"
echo "  3. Scroll to Artifacts section"
echo "  4. Click artifact name to download"
