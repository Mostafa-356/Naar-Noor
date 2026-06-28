#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
HTML Coverage Report Generator using ReportGenerator

Purpose: Invokes ReportGenerator to create interactive HTML coverage reports from Cobertura XML files
Reference: Requirement 1.3 - Configure HTML report generation with drill-down capability

Features:
  - Accepts coverage.cobertura.xml file(s) as input
  - Configures HTML report generation with drill-down to file-level coverage
  - Outputs reports to /coverage-reports/ directory
  - Supports merging multiple coverage files
  - Works in CI/CD and local development environments
  - Verbose logging for troubleshooting
"""

import sys
import os
import subprocess
import argparse
import logging
from pathlib import Path
from typing import List, Optional
from datetime import datetime

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)


class ReportGeneratorInvoker:
    """Manages ReportGenerator invocation for HTML coverage report generation."""
    
    def __init__(self, verbose: bool = False):
        self.verbose = verbose
        self.report_generator_cmd = "reportgenerator"
        if verbose:
            logger.setLevel(logging.DEBUG)
    
    def check_reportgenerator_installed(self) -> bool:
        """Check if ReportGenerator is installed as a global tool."""
        try:
            result = subprocess.run(
                [self.report_generator_cmd, "--help"],
                capture_output=True,
                text=True,
                timeout=5
            )
            return result.returncode == 0
        except (subprocess.TimeoutExpired, FileNotFoundError):
            return False
    
    def install_reportgenerator(self) -> bool:
        """Install ReportGenerator as a global dotnet tool."""
        logger.info("Installing ReportGenerator globally...")
        try:
            result = subprocess.run(
                ["dotnet", "tool", "install", "-g", "dotnet-reportgenerator-globaltool"],
                capture_output=True,
                text=True,
                timeout=60
            )
            if result.returncode == 0:
                logger.info("ReportGenerator installed successfully")
                return True
            else:
                logger.error(f"Installation failed: {result.stderr}")
                return False
        except Exception as e:
            logger.error(f"Failed to install ReportGenerator: {e}")
            return False
    
    def resolve_coverage_files(self, paths: List[str]) -> List[str]:
        """Resolve coverage file paths, handling wildcards and directories."""
        resolved_files = []
        
        for path_pattern in paths:
            path_obj = Path(path_pattern)
            
            # Handle wildcard patterns
            if '*' in path_pattern:
                matches = list(Path(path_obj.parent).glob(path_obj.name))
                for match in matches:
                    if match.name == "coverage.cobertura.xml" or match.suffix == ".xml":
                        resolved_files.append(str(match.resolve()))
            # Handle direct file path
            elif path_obj.is_file():
                if path_obj.name.endswith(".cobertura.xml") or path_obj.name == "coverage.cobertura.xml":
                    resolved_files.append(str(path_obj.resolve()))
            # Handle directory
            elif path_obj.is_dir():
                for xml_file in path_obj.rglob("coverage.cobertura.xml"):
                    resolved_files.append(str(xml_file.resolve()))
        
        # Remove duplicates
        return list(set(resolved_files))
    
    def validate_coverage_file(self, file_path: str) -> bool:
        """Validate that the file is a valid Cobertura coverage XML."""
        try:
            from xml.etree import ElementTree as ET
            tree = ET.parse(file_path)
            root = tree.getroot()
            
            # Verify it's a Cobertura coverage file
            if root.tag != "coverage":
                logger.error(f"File is not a valid Cobertura XML: {file_path}")
                return False
            
            # Count packages
            package_count = len(root.findall("package"))
            logger.debug(f"Valid coverage file: {file_path} (packages: {package_count})")
            return True
        except Exception as e:
            logger.error(f"Failed to parse coverage XML: {e}")
            return False
    
    def build_reportgenerator_command(
        self,
        report_paths: List[str],
        output_dir: str,
        report_types: str
    ) -> List[str]:
        """Build ReportGenerator command with all required arguments."""
        # Join multiple report paths with semicolon (ReportGenerator convention)
        reports_arg = ";".join(report_paths)
        
        cmd = [
            self.report_generator_cmd,
            f"-reports:{reports_arg}",
            f"-targetdir:{output_dir}",
            f"-reporttypes:{report_types}",
            "-assemblyfilters:+NaarNoor.Domain;+NaarNoor.Application;+NaarNoor.Infrastructure;+NaarNoor.API",
            "-filefilters:-*.Tests"
        ]
        
        return cmd
    
    def invoke_reportgenerator(
        self,
        report_paths: List[str],
        output_dir: str,
        report_types: str
    ) -> bool:
        """Execute ReportGenerator to generate HTML reports."""
        logger.info("Executing ReportGenerator...")
        
        cmd = self.build_reportgenerator_command(report_paths, output_dir, report_types)
        
        if self.verbose:
            logger.debug(f"Command: {' '.join(cmd)}")
        
        try:
            result = subprocess.run(
                cmd,
                capture_output=not self.verbose,
                text=True,
                timeout=120
            )
            
            if result.returncode != 0:
                logger.error(f"ReportGenerator failed with exit code {result.returncode}")
                if result.stderr:
                    logger.error(f"Error output: {result.stderr}")
                return False
            
            logger.info("ReportGenerator execution completed successfully")
            return True
        except subprocess.TimeoutExpired:
            logger.error("ReportGenerator execution timed out")
            return False
        except Exception as e:
            logger.error(f"ReportGenerator execution failed: {e}")
            return False
    
    def verify_report_output(self, output_dir: str) -> bool:
        """Verify that reports were generated successfully."""
        output_path = Path(output_dir)
        index_file = output_path / "index.html"
        
        if not index_file.exists():
            logger.error(f"Main report not found: {index_file}")
            return False
        
        logger.info(f"Index report generated: {index_file}")
        
        # Count generated files
        all_files = list(output_path.rglob("*"))
        file_count = len([f for f in all_files if f.is_file()])
        total_size = sum(f.stat().st_size for f in all_files if f.is_file()) / 1024
        
        logger.info(f"Total files generated: {file_count}")
        logger.info(f"Total size: {total_size:.2f} KB")
        
        return True
    
    def generate_reports(
        self,
        coverage_files: List[str],
        output_dir: str,
        report_types: str = "Html,HtmlSummary"
    ) -> bool:
        """Main method to orchestrate report generation."""
        logger.info("=" * 60)
        logger.info("HTML Coverage Report Generation")
        logger.info("=" * 60)
        
        # Step 1: Check ReportGenerator installation
        logger.info("[1/5] Checking ReportGenerator installation...")
        if not self.check_reportgenerator_installed():
            if not self.install_reportgenerator():
                logger.error("Failed to install ReportGenerator")
                return False
        
        # Step 2: Validate coverage files
        logger.info("[2/5] Validating coverage files...")
        all_valid = True
        for file_path in coverage_files:
            if not self.validate_coverage_file(file_path):
                all_valid = False
        
        if not all_valid:
            logger.error("One or more coverage files are invalid")
            return False
        
        logger.info(f"All {len(coverage_files)} coverage files are valid")
        
        # Step 3: Prepare output directory
        logger.info("[3/5] Preparing output directory...")
        output_path = Path(output_dir)
        output_path.mkdir(parents=True, exist_ok=True)
        
        # Clean old reports if they exist
        if output_path.exists() and list(output_path.glob("*")):
            logger.info("Clearing old reports...")
            for item in output_path.glob("*"):
                if item.is_file():
                    item.unlink()
                elif item.is_dir():
                    import shutil
                    shutil.rmtree(item)
        
        logger.info(f"Output directory ready: {output_dir}")
        
        # Step 4: Generate reports
        logger.info("[4/5] Generating HTML reports...")
        if not self.invoke_reportgenerator(coverage_files, output_dir, report_types):
            return False
        
        # Step 5: Verify output
        logger.info("[5/5] Verifying report output...")
        if not self.verify_report_output(output_dir):
            return False
        
        # Success message
        logger.info("=" * 60)
        logger.info("✅ REPORT GENERATION COMPLETE")
        logger.info("=" * 60)
        logger.info(f"Coverage Report Dashboard: {output_path / 'index.html'}")
        logger.info("Dashboard Features:")
        logger.info("  • View coverage metrics per layer")
        logger.info("  • Drill down to individual classes and methods")
        logger.info("  • Interactive coverage visualization")
        logger.info("  • Export coverage data to various formats")
        
        return True


def main():
    parser = argparse.ArgumentParser(
        description='Generate HTML coverage reports using ReportGenerator',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  # Generate report from single coverage file
  %(prog)s --coverage ./api-server/tests/TestResults/coverage.cobertura.xml

  # Generate report from multiple coverage files
  %(prog)s --coverage ./coverage-1.xml ./coverage-2.xml --output ./reports/

  # Generate report with verbose output
  %(prog)s --coverage ./coverage.cobertura.xml --verbose
        """
    )
    
    parser.add_argument(
        '--coverage',
        nargs='+',
        required=True,
        help='Path(s) to coverage.cobertura.xml file(s) or directory patterns'
    )
    
    parser.add_argument(
        '--output',
        default='./coverage-reports/',
        help='Output directory for HTML reports (default: ./coverage-reports/)'
    )
    
    parser.add_argument(
        '--report-types',
        default='Html,HtmlSummary',
        help='Report types to generate (default: Html,HtmlSummary)'
    )
    
    parser.add_argument(
        '--verbose',
        action='store_true',
        help='Enable verbose output'
    )
    
    args = parser.parse_args()
    
    # Resolve coverage files
    invoker = ReportGeneratorInvoker(verbose=args.verbose)
    coverage_files = invoker.resolve_coverage_files(args.coverage)
    
    if not coverage_files:
        logger.error("No coverage files found matching the provided paths")
        logger.error("To generate coverage files, run:")
        logger.error("  dotnet test --collect:\"XPlat Code Coverage\" --settings:coverlet.runsettings")
        return 1
    
    logger.info(f"Found {len(coverage_files)} coverage file(s)")
    for file_path in coverage_files:
        logger.info(f"  • {Path(file_path).name}")
    
    # Generate reports
    success = invoker.generate_reports(
        coverage_files,
        args.output,
        args.report_types
    )
    
    return 0 if success else 1


if __name__ == '__main__':
    sys.exit(main())
