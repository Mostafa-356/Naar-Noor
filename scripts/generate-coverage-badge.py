#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Coverage Badge Generator
Purpose: Generate SVG badges showing coverage percentage for README and documentation
Reference: Requirement 2.4 - Generate SVG badges for README showing current coverage percentage

Features:
  - Generates SVG badges for backend and frontend coverage
  - Color-codes badges based on coverage percentage
  - Suitable for embedding in README.md
  - Generates badges for overall and per-layer coverage
"""

import sys
import json
import argparse
from pathlib import Path
from typing import Dict, Any, Optional
import io

# Ensure stdout can handle UTF-8
if sys.stdout.encoding != 'utf-8':
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')


class CoverageBadgeGenerator:
    """Generates SVG coverage badges."""
    
    # Color mapping based on coverage percentage
    COLOR_MAP = {
        'excellent': '#4c1',   # Green (>= 90%)
        'good': '#dfb317',     # Gold (80-89%)
        'fair': '#fe7d37',     # Orange (70-79%)
        'poor': '#e05d44',     # Red (< 70%)
    }
    
    @staticmethod
    def get_color(coverage: float) -> str:
        """Get color for coverage percentage."""
        if coverage >= 90:
            return CoverageBadgeGenerator.COLOR_MAP['excellent']
        elif coverage >= 80:
            return CoverageBadgeGenerator.COLOR_MAP['good']
        elif coverage >= 70:
            return CoverageBadgeGenerator.COLOR_MAP['fair']
        else:
            return CoverageBadgeGenerator.COLOR_MAP['poor']
    
    @staticmethod
    def get_label(coverage: float) -> str:
        """Get status label for coverage percentage."""
        if coverage >= 90:
            return 'excellent'
        elif coverage >= 80:
            return 'good'
        elif coverage >= 70:
            return 'fair'
        else:
            return 'poor'
    
    @staticmethod
    def generate_svg_badge(label: str, coverage: float, filename: Optional[str] = None) -> str:
        """Generate SVG badge for coverage."""
        color = CoverageBadgeGenerator.get_color(coverage)
        coverage_text = f"{coverage:.1f}%"
        
        # SVG template
        svg = f'''<svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" width="140" height="20" role="img" aria-label="{label}: {coverage_text}">
  <title>{label}: {coverage_text}</title>
  <linearGradient id="s" x2="0" y2="100%">
    <stop offset="0" stop-color="#bbb"/>
    <stop offset="1" stop-color="#999"/>
  </linearGradient>
  <clipPath id="r">
    <rect width="140" height="20" rx="3" fill="#fff"/>
  </clipPath>
  <g clip-path="url(#r)">
    <rect width="110" height="20" fill="#555"/>
    <rect x="110" width="30" height="20" fill="{color}"/>
    <rect width="140" height="20" fill="url(#s)" opacity="0.1"/>
  </g>
  <g fill="#fff" text-anchor="middle" font-family="Verdana,Geneva,DejaVu Sans,sans-serif" text-rendering="geometricPrecision" font-size="11">
    <text aria-hidden="true" x="56" y="15" fill="#010101" fill-opacity="0.3" transform="scale(.1)" textLength="1000">{label}</text>
    <text x="56" y="14" transform="scale(.1)" fill="#fff" textLength="1000">{label}</text>
    <text aria-hidden="true" x="124" y="15" fill="#010101" fill-opacity="0.3" transform="scale(.1)" textLength="200">{coverage_text}</text>
    <text x="124" y="14" transform="scale(.1)" fill="#fff" textLength="200">{coverage_text}</text>
  </g>
</svg>'''
        
        return svg
    
    @staticmethod
    def load_coverage_report(file_path: str) -> Dict[str, Any]:
        """Load JSON coverage report."""
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                return json.load(f)
        except Exception as e:
            print(f"Error loading report {file_path}: {e}", file=sys.stderr)
            return {}
    
    @staticmethod
    def calculate_overall_coverage(results: Dict[str, Any]) -> float:
        """Calculate overall coverage from results."""
        if not results:
            return 0.0
        
        total_coverage = sum(data.get('coverage', 0) for data in results.values())
        return total_coverage / len(results) if results else 0.0
    
    def generate_badges(
        self,
        backend_report: Dict[str, Any],
        frontend_report: Dict[str, Any],
        output_dir: str
    ) -> None:
        """Generate all coverage badges."""
        Path(output_dir).mkdir(parents=True, exist_ok=True)
        
        # Backend overall coverage
        backend_results = backend_report.get('results', {})
        backend_overall = self.calculate_overall_coverage(backend_results)
        
        backend_svg = self.generate_svg_badge('backend coverage', backend_overall)
        with open(f"{output_dir}/coverage-backend.svg", 'w', encoding='utf-8') as f:
            f.write(backend_svg)
        
        # Backend per-layer badges
        for layer, data in backend_results.items():
            coverage = data.get('coverage', 0)
            layer_svg = self.generate_svg_badge(f'{layer} coverage', coverage)
            with open(f"{output_dir}/coverage-backend-{layer.lower()}.svg", 'w', encoding='utf-8') as f:
                f.write(layer_svg)
        
        # Frontend overall coverage
        frontend_results = frontend_report.get('results', {})
        frontend_overall = self.calculate_overall_coverage(frontend_results)
        
        frontend_svg = self.generate_svg_badge('frontend coverage', frontend_overall)
        with open(f"{output_dir}/coverage-frontend.svg", 'w', encoding='utf-8') as f:
            f.write(frontend_svg)
        
        # Frontend per-layer badges
        for layer, data in frontend_results.items():
            coverage = data.get('coverage', 0)
            layer_svg = self.generate_svg_badge(f'{layer} coverage', coverage)
            with open(f"{output_dir}/coverage-frontend-{layer.lower()}.svg", 'w', encoding='utf-8') as f:
                f.write(layer_svg)
        
        # Combined overall coverage
        all_coverages = [backend_overall] + [d.get('coverage', 0) for d in backend_results.values()] + \
                        [frontend_overall] + [d.get('coverage', 0) for d in frontend_results.values()]
        overall_avg = sum(all_coverages) / len(all_coverages) if all_coverages else 0.0
        
        overall_svg = self.generate_svg_badge('coverage', overall_avg)
        with open(f"{output_dir}/coverage-overall.svg", 'w', encoding='utf-8') as f:
            f.write(overall_svg)
        
        print(f"Generated {len(backend_results) + len(frontend_results) + 3} coverage badges in {output_dir}")


def main():
    parser = argparse.ArgumentParser(
        description='Generate coverage badges from reports'
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
        required=True,
        help='Output directory for badge SVG files'
    )
    
    args = parser.parse_args()
    
    generator = CoverageBadgeGenerator()
    
    # Load reports
    backend_report = generator.load_coverage_report(args.backend_report)
    frontend_report = generator.load_coverage_report(args.frontend_report)
    
    # Generate badges
    generator.generate_badges(backend_report, frontend_report, args.output)
    
    return 0


if __name__ == '__main__':
    sys.exit(main())
