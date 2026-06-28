#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
GitHub PR Comment Generator for Coverage Gate Results
Purpose: Generate enhanced PR comment with coverage summary, status, links, and recommendations
Reference: Requirement 2.2 - Create PR comment workflow (Task 2.5)

Features:
  - Per-layer coverage summary table
  - Pass/fail status indicators
  - Links to detailed HTML coverage reports
  - Historical coverage comparison (if available)
  - Actionable recommendations based on results
  - Visual indicators and emoji for quick scanning
"""

import sys
import json
import argparse
from pathlib import Path
from typing import Dict, Any, Optional, Tuple
from datetime import datetime
import io

# Ensure stdout can handle UTF-8
if sys.stdout.encoding != 'utf-8':
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')


class PRCommentGenerator:
    """Generates enhanced GitHub PR comments with coverage details."""
    
    def __init__(self, backend_report: Dict[str, Any] = None, frontend_report: Dict[str, Any] = None):
        self.backend_report = backend_report or {}
        self.frontend_report = frontend_report or {}
        self.backend_passed = self.backend_report.get('passed', False)
        self.frontend_passed = self.frontend_report.get('passed', False)
    
    def format_layer_row(
        self,
        layer_name: str,
        coverage: float,
        threshold: float,
        passed: bool
    ) -> str:
        """Format a single layer row for the coverage table."""
        status = "✅" if passed else "❌"
        gap = coverage - threshold
        gap_indicator = f"{gap:+.2f}%" if gap != 0 else "met"
        
        # Color coding for visual clarity
        if passed:
            coverage_badge = f"**{coverage:.2f}%**"
        else:
            coverage_badge = f"**{coverage:.2f}%** ⚠️"
        
        return f"| {status} {layer_name} | {coverage_badge} | {threshold:.2f}% | {gap_indicator} |"
    
    def generate_backend_section(self) -> str:
        """Generate backend coverage section."""
        section = "### 🔷 Backend Coverage (.NET)\n\n"
        
        backend_results = self.backend_report.get('results', {})
        
        if not backend_results:
            section += "_No backend coverage data available_\n"
            return section
        
        # Coverage table
        section += "| Status | Layer | Coverage | Threshold | Gap |\n"
        section += "|--------|-------|----------|-----------|-----|\n"
        
        layers_passed = 0
        total_layers = len(backend_results)
        
        for layer, data in backend_results.items():
            coverage = data.get('coverage', 0)
            threshold = data.get('threshold', 0)
            passed = data.get('passed', False)
            
            if passed:
                layers_passed += 1
            
            # Format layer name nicely
            layer_display = layer.replace('NaarNoor.', '').replace('_', ' ')
            section += self.format_layer_row(layer_display, coverage, threshold, passed) + "\n"
        
        section += f"\n**Backend Progress:** {layers_passed}/{total_layers} layers passed\n\n"
        
        # Backend status summary
        if self.backend_passed:
            section += "✅ **Backend coverage requirements met!**\n"
        else:
            section += "❌ **Backend coverage requirements NOT met**\n\n"
            section += "**Failed Layers:**\n"
            for layer, data in backend_results.items():
                if not data.get('passed', False):
                    gap = data.get('coverage', 0) - data.get('threshold', 0)
                    layer_display = layer.replace('NaarNoor.', '')
                    section += f"- {layer_display}: {abs(gap):.2f}% below threshold\n"
        
        return section
    
    def generate_frontend_section(self) -> str:
        """Generate frontend coverage section."""
        section = "### 🔷 Frontend Coverage (Angular)\n\n"
        
        frontend_results = self.frontend_report.get('results', {})
        
        if not frontend_results:
            section += "_No frontend coverage data available_\n"
            return section
        
        # Coverage table
        section += "| Status | Layer | Coverage | Threshold | Gap |\n"
        section += "|--------|-------|----------|-----------|-----|\n"
        
        layers_passed = 0
        total_layers = len(frontend_results)
        
        for layer, data in frontend_results.items():
            coverage = data.get('coverage', 0)
            threshold = data.get('threshold', 0)
            passed = data.get('passed', False)
            
            if passed:
                layers_passed += 1
            
            section += self.format_layer_row(layer, coverage, threshold, passed) + "\n"
        
        section += f"\n**Frontend Progress:** {layers_passed}/{total_layers} layers passed\n\n"
        
        # Frontend status summary
        if self.frontend_passed:
            section += "✅ **Frontend coverage requirements met!**\n"
        else:
            section += "❌ **Frontend coverage requirements NOT met**\n\n"
            section += "**Failed Layers:**\n"
            for layer, data in frontend_results.items():
                if not data.get('passed', False):
                    gap = data.get('coverage', 0) - data.get('threshold', 0)
                    section += f"- {layer}: {abs(gap):.2f}% below threshold\n"
        
        return section
    
    def generate_links_section(self) -> str:
        """Generate section with links to detailed reports."""
        section = "### 📊 Detailed Reports\n\n"
        
        section += "Access detailed coverage information:\n\n"
        section += "- 📈 [Backend Coverage Report](https://github.com/${{ github.repository }}/actions/runs/${{ github.run_id }}#artifacts) "
        section += "(HTML drill-down available in artifacts)\n"
        section += "- 📈 [Frontend Coverage Report](https://github.com/${{ github.repository }}/actions/runs/${{ github.run_id }}#artifacts) "
        section += "(HTML drill-down available in artifacts)\n"
        section += "- 📋 [Coverage Summary](https://github.com/${{ github.repository }}/actions/runs/${{ github.run_id }}#artifacts)\n\n"
        
        section += "> 💡 **Tip:** Download the coverage reports from the workflow artifacts to view interactive HTML reports with file-level drill-down.\n\n"
        
        return section
    
    def generate_recommendations_section(self) -> str:
        """Generate actionable recommendations based on results."""
        section = "### 💡 Recommendations\n\n"
        
        overall_passed = self.backend_passed and self.frontend_passed
        
        if overall_passed:
            section += "✅ **All coverage gates passed!**\n\n"
            section += "**Next steps:**\n"
            section += "- ✅ This PR can be merged\n"
            section += "- 📈 Maintain current coverage standards in future PRs\n"
            section += "- 🎯 Consider increasing coverage thresholds for critical layers\n"
            section += "- 📊 Monitor coverage trends over time\n"
        else:
            section += "❌ **Coverage gates failed - action required before merge**\n\n"
            section += "**To fix coverage issues:**\n\n"
            
            if not self.backend_passed:
                section += "**Backend:**\n"
                section += "1. Run tests locally with coverage:\n"
                section += "   ```bash\n"
                section += "   cd api-server\n"
                section += "   dotnet test --collect:\"XPlat Code Coverage\"\n"
                section += "   ```\n"
                section += "2. Review HTML coverage report at `coverage-reports/index.html`\n"
                section += "3. Identify uncovered code paths\n"
                section += "4. Write unit tests for low-coverage methods\n"
                section += "5. Commit and push - this workflow will run automatically\n\n"
            
            if not self.frontend_passed:
                section += "**Frontend:**\n"
                section += "1. Run tests locally with coverage:\n"
                section += "   ```bash\n"
                section += "   npm run test:ci\n"
                section += "   ```\n"
                section += "2. Review HTML coverage report in `coverage/` directory\n"
                section += "3. Add Jasmine/Karma tests for uncovered components/services\n"
                section += "4. Commit and push - this workflow will run automatically\n\n"
            
            section += "**General:**\n"
            section += "- 📖 See [Testing Guide](../../docs/TESTING.md) for testing best practices\n"
            section += "- 📚 See [Property Testing Guide](../../docs/TESTING_PROPERTIES.md) for advanced patterns\n"
            section += "- ⚙️ Questions? Contact the QA team\n"
        
        return section
    
    def generate_comparison_section(self, previous_coverage: Optional[Dict[str, float]] = None) -> str:
        """Generate historical comparison section if previous data available."""
        if not previous_coverage:
            return ""
        
        section = "### 📉 Coverage Trend\n\n"
        section += "Comparison with previous commit:\n\n"
        
        section += "| Layer | Current | Previous | Change |\n"
        section += "|-------|---------|----------|--------|\n"
        
        # Show comparisons
        if 'backend' in previous_coverage:
            prev_backend = previous_coverage['backend']
            curr_backend = self.backend_report.get('results', {}).get('overall', {}).get('coverage', 0)
            change = curr_backend - prev_backend
            direction = "📈" if change >= 0 else "📉"
            section += f"| Backend | {curr_backend:.2f}% | {prev_backend:.2f}% | {direction} {change:+.2f}% |\n"
        
        if 'frontend' in previous_coverage:
            prev_frontend = previous_coverage['frontend']
            curr_frontend = self.frontend_report.get('results', {}).get('overall', {}).get('coverage', 0)
            change = curr_frontend - prev_frontend
            direction = "📈" if change >= 0 else "📉"
            section += f"| Frontend | {curr_frontend:.2f}% | {prev_frontend:.2f}% | {direction} {change:+.2f}% |\n"
        
        section += "\n"
        return section
    
    def generate_full_comment(self, include_details: bool = True) -> str:
        """Generate the complete PR comment."""
        lines = []
        
        overall_passed = self.backend_passed and self.frontend_passed
        
        # Header with main status
        if overall_passed:
            lines.append("# ✅ Coverage Gates Passed\n")
            lines.append("All code layers have met their coverage requirements. This PR can be merged.\n")
        else:
            lines.append("# ❌ Coverage Gates Failed\n")
            lines.append("One or more code layers have not met their coverage requirements. ")
            lines.append("Please add tests before merging.\n")
        
        lines.append(f"_Report generated at {datetime.now().strftime('%Y-%m-%d %H:%M:%S UTC')}_\n")
        
        if include_details:
            # Coverage sections
            lines.append("\n---\n")
            lines.append(self.generate_backend_section())
            lines.append("\n")
            lines.append(self.generate_frontend_section())
            
            # Links section
            lines.append("\n")
            lines.append(self.generate_links_section())
            
            # Recommendations
            lines.append("\n")
            lines.append(self.generate_recommendations_section())
            
            # Footer
            lines.append("\n---\n")
            lines.append("*Coverage thresholds are enforced to maintain code quality standards.*\n")
            lines.append("*For more information, see [Testing Documentation](../../docs/TESTING.md)*\n")
        
        return "".join(lines)


def load_json_report(file_path: str) -> Dict[str, Any]:
    """Load JSON coverage report."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            return json.load(f)
    except Exception as e:
        print(f"Error loading report {file_path}: {e}", file=sys.stderr)
        return {}


def main():
    parser = argparse.ArgumentParser(
        description='Generate GitHub PR comment for coverage gate results'
    )
    parser.add_argument(
        '--backend-report',
        help='Backend coverage report JSON file'
    )
    parser.add_argument(
        '--frontend-report',
        help='Frontend coverage report JSON file'
    )
    parser.add_argument(
        '--summary-report',
        help='Summary report file (from generate-coverage-report.py)'
    )
    parser.add_argument(
        '--output',
        help='Output markdown file for PR comment'
    )
    parser.add_argument(
        '--include-details',
        action='store_true',
        default=True,
        help='Include detailed coverage tables (default: true)'
    )
    
    args = parser.parse_args()
    
    # Load reports
    backend_report = {}
    frontend_report = {}
    
    if args.backend_report:
        backend_report = load_json_report(args.backend_report)
    
    if args.frontend_report:
        frontend_report = load_json_report(args.frontend_report)
    
    # Generate comment
    generator = PRCommentGenerator(backend_report, frontend_report)
    comment = generator.generate_full_comment(include_details=args.include_details)
    
    # Output
    if args.output:
        with open(args.output, 'w', encoding='utf-8') as f:
            f.write(comment)
        print(f"PR comment written to {args.output}")
    else:
        print(comment)
    
    return 0


if __name__ == '__main__':
    sys.exit(main())
