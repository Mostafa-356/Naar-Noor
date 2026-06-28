#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Coverage Comparison Report Generator for GitHub PR Comments
Purpose: Generates detailed markdown report comparing backend and frontend coverage against thresholds
Reference: Requirement 2.2 - Provide detailed report showing gap size

Features:
  - Per-layer coverage comparison
  - Visual pass/fail indicators
  - Gap analysis and recommendations
  - Markdown formatted for GitHub PR comments
"""

import sys
import json
import argparse
from pathlib import Path
from typing import Dict, Any, Optional
from datetime import datetime
import io

# Ensure stdout can handle UTF-8
if sys.stdout.encoding != 'utf-8':
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')


class CoverageReportGenerator:
    """Generates markdown coverage comparison reports."""
    
    def __init__(self):
        self.backend_report = {}
        self.frontend_report = {}
    
    def load_json_report(self, file_path: str, report_type: str = 'backend') -> Dict[str, Any]:
        """Load JSON coverage report."""
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                data = json.load(f)
                # If the file contains nested backend/frontend structure, extract the relevant part
                if 'backend' in data and 'frontend' in data:
                    return data.get(report_type, {})
                return data
        except Exception as e:
            print(f"Error loading report {file_path}: {e}", file=sys.stderr)
            return {}
    
    def format_coverage_row(
        self,
        layer: str,
        coverage: float,
        threshold: float,
        passed: bool
    ) -> str:
        """Format a single coverage row for markdown table."""
        status = "✅ PASS" if passed else "❌ FAIL"
        gap = coverage - threshold
        gap_str = f"{gap:+.2f}%" if not passed else "—"
        
        return f"| {layer} | {coverage:.2f}% | {threshold:.2f}% | {gap_str} | {status} |"
    
    def generate_markdown_report(self) -> str:
        """Generate full markdown report."""
        report_lines = []
        
        # Header
        report_lines.append("# 📊 Coverage Gate Report\n")
        report_lines.append(f"_Generated at {datetime.now().strftime('%Y-%m-%d %H:%M:%S UTC')}_\n")
        
        # Overall status
        backend_passed = self.backend_report.get('passed', False)
        frontend_passed = self.frontend_report.get('passed', False)
        overall_passed = backend_passed and frontend_passed
        
        if overall_passed:
            report_lines.append("## ✅ All Coverage Gates Passed\n")
            report_lines.append("All code layers have met their coverage requirements.\n")
        else:
            report_lines.append("## ❌ Coverage Gates Failed\n")
            report_lines.append("One or more code layers have failed their coverage requirements. Please increase test coverage before merging.\n")
        
        # Backend Coverage Section
        report_lines.append("\n### Backend Coverage (.NET)\n")
        
        backend_results = self.backend_report.get('results', {})
        
        if backend_results:
            report_lines.append("| Layer | Coverage | Threshold | Gap | Status |")
            report_lines.append("|-------|----------|-----------|-----|--------|")
            
            for layer, data in backend_results.items():
                coverage = data.get('coverage', 0)
                threshold = data.get('threshold', 0)
                passed = data.get('passed', False)
                
                report_lines.append(
                    self.format_coverage_row(layer, coverage, threshold, passed)
                )
            
            # Backend details
            if not backend_passed:
                report_lines.append("\n**Action Required:**")
                report_lines.append("- Add unit tests for uncovered code paths")
                report_lines.append("- Focus on Domain layer (highest threshold: 85%)")
                report_lines.append("- Review design for testability issues")
        else:
            report_lines.append("_No backend coverage data available_\n")
        
        # Frontend Coverage Section
        report_lines.append("\n### Frontend Coverage (Angular)\n")
        
        frontend_results = self.frontend_report.get('results', {})
        
        if frontend_results:
            report_lines.append("| Layer | Coverage | Threshold | Gap | Status |")
            report_lines.append("|-------|----------|-----------|-----|--------|")
            
            for layer, data in frontend_results.items():
                coverage = data.get('coverage', 0)
                threshold = data.get('threshold', 0)
                passed = data.get('passed', False)
                
                report_lines.append(
                    self.format_coverage_row(layer, coverage, threshold, passed)
                )
            
            # Frontend details
            if not frontend_passed:
                report_lines.append("\n**Action Required:**")
                report_lines.append("- Add Jasmine/Karma tests for Angular services and components")
                report_lines.append("- Focus on Services layer (80% threshold)")
                report_lines.append("- Ensure all component interactions are tested")
        else:
            report_lines.append("_No frontend coverage data available_\n")
        
        # Detailed Recommendations
        report_lines.append("\n### 💡 Recommendations\n")
        
        if overall_passed:
            report_lines.append("✅ **Next Steps:**")
            report_lines.append("- You may now merge this PR")
            report_lines.append("- Continue maintaining high coverage standards")
            report_lines.append("- Monitor coverage trends over time")
        else:
            report_lines.append("❌ **Before Merging:**")
            report_lines.append("1. **Run tests locally:**")
            report_lines.append("   - Backend: `dotnet test --collect:\"XPlat Code Coverage\"`")
            report_lines.append("   - Frontend: `npm run test:ci`")
            report_lines.append("2. **Analyze coverage gaps:**")
            report_lines.append("   - Review HTML coverage reports in artifacts")
            report_lines.append("   - Identify untested code paths")
            report_lines.append("3. **Add tests:**")
            report_lines.append("   - Write unit tests for low-coverage methods")
            report_lines.append("   - Run coverage validation again")
            report_lines.append("4. **Push updated tests:**")
            report_lines.append("   - This workflow will run automatically")
            report_lines.append("   - PR will merge once all gates pass")
        
        # Footer
        report_lines.append("\n---")
        report_lines.append("*Coverage thresholds are enforced to maintain code quality.*")
        report_lines.append("*For questions, see [Testing Documentation](../../docs/TESTING_COVERAGE.md)*\n")
        
        return "\n".join(report_lines)
    
    def generate_summary_table(self) -> str:
        """Generate concise summary table."""
        summary = "## Coverage Summary\n\n"
        
        backend_passed = self.backend_report.get('passed', False)
        frontend_passed = self.frontend_report.get('passed', False)
        
        summary += "| Layer Type | Status |\n"
        summary += "|------------|--------|\n"
        summary += f"| Backend | {'✅ PASS' if backend_passed else '❌ FAIL'} |\n"
        summary += f"| Frontend | {'✅ PASS' if frontend_passed else '❌ FAIL'} |\n\n"
        
        return summary


def main():
    parser = argparse.ArgumentParser(
        description='Generate coverage comparison report'
    )
    parser.add_argument(
        '--backend-report',
        required=True,
        help='Backend coverage report JSON file'
    )
    parser.add_argument(
        '--frontend-report',
        required=True,
        help='Frontend coverage report JSON file'
    )
    parser.add_argument(
        '--output',
        help='Output markdown file'
    )
    
    args = parser.parse_args()
    
    generator = CoverageReportGenerator()
    
    # Load reports with correct report types
    generator.backend_report = generator.load_json_report(args.backend_report, 'backend')
    generator.frontend_report = generator.load_json_report(args.frontend_report, 'frontend')
    
    # Generate report
    report = generator.generate_markdown_report()
    
    # Output
    if args.output:
        with open(args.output, 'w', encoding='utf-8') as f:
            f.write(report)
        print(f"Report written to {args.output}")
    else:
        print(report)
    
    return 0


if __name__ == '__main__':
    sys.exit(main())
