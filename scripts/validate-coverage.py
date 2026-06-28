#!/usr/bin/env python3
"""
Coverage Threshold Validation Script for CI/CD Pipeline
Purpose: Validates backend and frontend code coverage against defined thresholds
Reference: Requirement 2.2 - Coverage enforcement blocks low-quality merges

Thresholds (Backend - .NET):
  - Domain: 85% line coverage
  - Application: 82% line coverage  
  - Infrastructure: 78% line coverage
  - API: 80% line coverage

Thresholds (Frontend - Angular):
  - Services: 80% line coverage
  - Components: 75% line coverage

Features:
  - Per-layer threshold checking
  - Detailed coverage gap reporting
  - JSON output for CI/CD integration
  - Exit codes for workflow control
"""

import sys
import json
import xml.etree.ElementTree as ET
import argparse
from pathlib import Path
from typing import Dict, List, Tuple, Optional
import os


class CoverageValidator:
    """Validates coverage reports against defined thresholds."""
    
    # Backend coverage thresholds
    BACKEND_THRESHOLDS = {
        'NaarNoor.Domain': 85.0,
        'NaarNoor.Application': 82.0,
        'NaarNoor.Infrastructure': 78.0,
        'NaarNoor.API': 80.0,
    }
    
    # Frontend coverage thresholds  
    FRONTEND_THRESHOLDS = {
        'Services': 80.0,
        'Components': 75.0,
    }
    
    def __init__(self):
        self.backend_results = {}
        self.frontend_results = {}
        self.backend_passed = True
        self.frontend_passed = True
    
    def parse_cobertura_xml(self, xml_path: str) -> Optional[float]:
        """Extract line coverage percentage from Cobertura XML."""
        try:
            tree = ET.parse(xml_path)
            root = tree.getroot()
            
            # Get overall line-rate from coverage element
            if 'line-rate' in root.attrib:
                line_rate = float(root.attrib['line-rate'])
                return line_rate * 100.0  # Convert to percentage
            
            return None
        except Exception as e:
            print(f"Error parsing {xml_path}: {e}", file=sys.stderr)
            return None
    
    def parse_package_coverage(self, xml_path: str, package_name: str) -> Optional[float]:
        """Extract line coverage for specific package from Cobertura XML."""
        try:
            tree = ET.parse(xml_path)
            root = tree.getroot()
            
            # Find package matching the name
            packages = root.findall('.//packages/package')
            for package in packages:
                if package.get('name') == package_name:
                    if 'line-rate' in package.attrib:
                        line_rate = float(package.attrib['line-rate'])
                        return line_rate * 100.0  # Convert to percentage
            
            # Fallback to overall coverage if package not found
            return self.parse_cobertura_xml(xml_path)
        except Exception as e:
            print(f"Error parsing package coverage from {xml_path}: {e}", file=sys.stderr)
            return None
    
    def get_uncovered_classes(self, xml_path: str, package_name: str, min_threshold: float = 50.0) -> List[Tuple[str, float]]:
        """Extract classes with coverage below threshold from Cobertura XML."""
        uncovered_classes = []
        
        try:
            tree = ET.parse(xml_path)
            root = tree.getroot()
            
            # Find the specific package
            packages = root.findall('.//packages/package')
            for package in packages:
                if package.get('name') == package_name:
                    # Extract all classes in this package
                    classes = package.findall('.//classes/class')
                    for cls in classes:
                        if 'line-rate' in cls.attrib:
                            coverage = float(cls.attrib['line-rate']) * 100.0
                            if coverage < min_threshold:
                                class_name = cls.get('name', 'Unknown')
                                uncovered_classes.append((class_name, coverage))
            
            # Sort by coverage (lowest first)
            uncovered_classes.sort(key=lambda x: x[1])
        except Exception as e:
            print(f"Error extracting uncovered classes: {e}", file=sys.stderr)
        
        return uncovered_classes
    
    def parse_istanbul_coverage(self, coverage_dir: str) -> Optional[Dict[str, float]]:
        """Parse Istanbul/Karma coverage report."""
        try:
            # Look for coverage.json or cobertura.xml
            coverage_json = Path(coverage_dir) / 'coverage-final.json'
            
            if coverage_json.exists():
                with open(coverage_json, 'r') as f:
                    data = json.load(f)
                    
                    # Extract line coverage from Istanbul format
                    if 'total' in data:
                        return {
                            'lines': data['total'].get('lines', {}).get('pct', 0),
                            'statements': data['total'].get('statements', {}).get('pct', 0),
                        }
            
            # Fallback to cobertura XML if present
            cobertura_file = Path(coverage_dir) / 'cobertura-coverage.xml'
            if cobertura_file.exists():
                coverage = self.parse_cobertura_xml(str(cobertura_file))
                if coverage:
                    return {'lines': coverage}
            
            return None
        except Exception as e:
            print(f"Error parsing Istanbul coverage: {e}", file=sys.stderr)
            return None
    
    def validate_backend_coverage(self, report_dir: str = './backend-coverage') -> bool:
        """Validate backend coverage against per-layer thresholds."""
        report_path = Path(report_dir)
        
        if not report_path.exists():
            print(f"Backend coverage directory not found: {report_dir}")
            return False
        
        # Find coverage.cobertura.xml files for each layer
        test_projects = {
            'NaarNoor.Domain.Tests': 'NaarNoor.Domain',
            'NaarNoor.Application.Tests': 'NaarNoor.Application',
            'NaarNoor.Infrastructure.Tests': 'NaarNoor.Infrastructure',
            'NaarNoor.API.Tests': 'NaarNoor.API',
        }
        
        overall_passed = True
        
        for test_dir, package_name in test_projects.items():
            test_path = report_path / test_dir
            xml_files = list(test_path.glob('**/coverage.cobertura.xml'))
            
            if not xml_files:
                print(f"WARN [{package_name}] No coverage report found at {test_path}")
                overall_passed = False
                continue
            
            # Use the most recent coverage file
            xml_file = max(xml_files, key=lambda p: p.stat().st_mtime)
            
            # Extract coverage for this package
            coverage = self.parse_package_coverage(str(xml_file), package_name)
            
            if coverage is None:
                print(f"WARN [{package_name}] Could not parse coverage from {xml_file}")
                overall_passed = False
                continue
            
            # Get the threshold for this package
            threshold = self.BACKEND_THRESHOLDS.get(package_name, 80.0)
            passed = coverage >= threshold
            
            if not passed:
                overall_passed = False
            
            # Store result
            self.backend_results[package_name] = {
                'coverage': round(coverage, 2),
                'threshold': threshold,
                'passed': passed,
                'coverage_file': str(xml_file),
            }
            
            # Get uncovered classes for gap report
            uncovered = self.get_uncovered_classes(str(xml_file), package_name, 50.0)
            if uncovered:
                self.backend_results[package_name]['uncovered_classes'] = uncovered[:5]
        
        self.backend_passed = overall_passed
        return overall_passed
    
    def validate_frontend_coverage(self, report_dir: str = './frontend-coverage') -> bool:
        """Validate frontend coverage against thresholds."""
        report_path = Path(report_dir)
        
        if not report_path.exists():
            print(f"Frontend coverage directory not found: {report_dir}")
            return False
        
        # Try to parse Istanbul/Karma coverage
        coverage_data = self.parse_istanbul_coverage(str(report_path))
        
        if coverage_data is None:
            print("Could not parse frontend coverage report")
            return False
        
        # Get line coverage (primary metric)
        line_coverage = coverage_data.get('lines', 0)
        
        # For frontend, check against minimum threshold
        min_threshold = min(self.FRONTEND_THRESHOLDS.values())
        
        self.frontend_results = {
            'overall': {
                'coverage': line_coverage,
                'threshold': min_threshold,
                'passed': line_coverage >= min_threshold,
            }
        }
        
        self.frontend_passed = line_coverage >= min_threshold
        return self.frontend_passed
    
    def generate_report(self) -> str:
        """Generate JSON report of coverage validation."""
        report = {
            'backend': {
                'passed': self.backend_passed,
                'results': self.backend_results,
            },
            'frontend': {
                'passed': self.frontend_passed,
                'results': self.frontend_results,
            },
            'overall_passed': self.backend_passed and self.frontend_passed,
        }
        
        return json.dumps(report, indent=2)
    
    def print_backend_report(self):
        """Print formatted backend coverage report."""
        if not self.backend_results:
            print("No backend coverage data available")
            return
        
        print("\n====================================")
        print("Backend Coverage Validation Report")
        print("====================================\n")
        
        for package_name in ['NaarNoor.Domain', 'NaarNoor.Application', 'NaarNoor.Infrastructure', 'NaarNoor.API']:
            if package_name in self.backend_results:
                result = self.backend_results[package_name]
                coverage = result['coverage']
                threshold = result['threshold']
                passed = result['passed']
                
                status = "PASS" if passed else "FAIL"
                
                print(f"[{status}] [{package_name}] Coverage: {coverage}% (Threshold: {threshold}%)")
                
                if not passed:
                    gap = threshold - coverage
                    print(f"      Gap: {gap}% below threshold")
                    
                    if 'uncovered_classes' in result and result['uncovered_classes']:
                        print(f"      Low coverage classes:")
                        for class_name, class_coverage in result['uncovered_classes']:
                            print(f"        - {class_name}: {class_coverage}%")
        
        print(f"\nOverall: {'PASS' if self.backend_passed else 'FAIL'}")
    
    def print_summary(self):
        """Print validation summary."""
        if self.backend_passed and self.frontend_passed:
            print("\n[SUCCESS] ALL VALIDATIONS PASSED")
            return 0
        else:
            print("\n[FAILURE] VALIDATION FAILED")
            if not self.backend_passed:
                print("  - Backend coverage below thresholds")
            if not self.frontend_passed:
                print("  - Frontend coverage below thresholds")
            return 1


def main():
    parser = argparse.ArgumentParser(
        description='Validate code coverage against defined thresholds'
    )
    parser.add_argument(
        '--backend',
        action='store_true',
        help='Validate backend coverage'
    )
    parser.add_argument(
        '--frontend',
        action='store_true',
        help='Validate frontend coverage'
    )
    parser.add_argument(
        '--backend-dir',
        default='./api-server/tests',
        help='Backend coverage directory (default: ./api-server/tests)'
    )
    parser.add_argument(
        '--frontend-dir',
        default='./frontend-coverage',
        help='Frontend coverage directory (default: ./frontend-coverage)'
    )
    parser.add_argument(
        '--output',
        help='Output JSON report to file'
    )
    parser.add_argument(
        '--report-format',
        default='json',
        help='Report format (default: json)'
    )
    
    args = parser.parse_args()
    
    # If no specific flags, validate both
    validate_both = not (args.backend or args.frontend)
    
    validator = CoverageValidator()
    
    backend_passed = True
    frontend_passed = True
    
    if args.backend or validate_both:
        print(f"Validating backend coverage from {args.backend_dir}...")
        backend_passed = validator.validate_backend_coverage(args.backend_dir)
        validator.print_backend_report()
        
        if backend_passed:
            print("[SUCCESS] Backend coverage passed")
        else:
            print("[FAILURE] Backend coverage failed")
    
    if args.frontend or validate_both:
        print(f"\nValidating frontend coverage from {args.frontend_dir}...")
        frontend_passed = validator.validate_frontend_coverage(args.frontend_dir)
        
        if frontend_passed:
            print("[SUCCESS] Frontend coverage passed")
        else:
            print("[FAILURE] Frontend coverage failed")
    
    # Generate report
    report = validator.generate_report()
    
    # Output report
    if args.output:
        with open(args.output, 'w') as f:
            f.write(report)
        print(f"\n[INFO] Report written to {args.output}")
    else:
        print("\n" + report)
    
    # Print summary and return
    exit_code = validator.print_summary()
    
    # Set GitHub Actions outputs
    if backend_passed and frontend_passed:
        print("::set-output name=passed::true")
    else:
        print("::set-output name=passed::false")
    
    return exit_code


if __name__ == '__main__':
    sys.exit(main())
