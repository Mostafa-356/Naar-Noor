-- Phase 2 Task 2.1: Row-Level Security (RLS) Implementation
-- Naar-Noor API Database
-- Date: June 28, 2026
-- Purpose: Enable RLS on all tables and create policies for data isolation

-- ============================================================================
-- PREREQUISITE: Tables Must Exist First!
-- ============================================================================
-- Before executing this script, ensure all database tables are created by running:
-- 
-- 1. Execute: Database/0_create_tables.sql (in Supabase SQL Editor)
--    This creates all 7 tables
--
-- 2. Then execute this script: Database/2_1_rls_implementation.sql
--
-- ============================================================================
-- STEP 1: Enable RLS on All Tables
-- ============================================================================

-- Check if tables exist before enabling RLS
ALTER TABLE IF EXISTS "Orders" ENABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS "Reservations" ENABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS "MenuItems" ENABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS "Chefs" ENABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS "Reviews" ENABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS "ContactInquiries" ENABLE ROW LEVEL SECURITY;
ALTER TABLE IF EXISTS "OrderItems" ENABLE ROW LEVEL SECURITY;

-- Verify RLS enabled on all tables
SELECT tablename, rowsecurity 
FROM pg_tables 
WHERE schemaname = 'public' 
  AND tablename IN ('Orders', 'Reservations', 'MenuItems', 'Chefs', 'Reviews', 'ContactInquiries', 'OrderItems')
ORDER BY tablename;

-- ============================================================================
-- STEP 2: Create RLS Policies for Orders Table
-- ============================================================================

-- Policy 1: Users can view their own orders
CREATE POLICY "Users can view own orders" ON "Orders"
  FOR SELECT
  USING ("Email" = auth.jwt() ->> 'email');

-- Policy 2: Users can create orders
CREATE POLICY "Users can create orders" ON "Orders"
  FOR INSERT
  WITH CHECK (true);

-- Policy 3: Users can update their own orders
CREATE POLICY "Users can update own orders" ON "Orders"
  FOR UPDATE
  USING ("Email" = auth.jwt() ->> 'email');

-- ============================================================================
-- STEP 3: Create RLS Policies for Reservations Table
-- ============================================================================

-- Policy 4: Users can view their own reservations
CREATE POLICY "Users can view own reservations" ON "Reservations"
  FOR SELECT
  USING ("Email" = auth.jwt() ->> 'email');

-- Policy 5: Users can create reservations
CREATE POLICY "Users can create reservations" ON "Reservations"
  FOR INSERT
  WITH CHECK (true);

-- Policy 6: Users can update their own reservations
CREATE POLICY "Users can update own reservations" ON "Reservations"
  FOR UPDATE
  USING ("Email" = auth.jwt() ->> 'email');

-- Policy 7: Users can delete their own reservations
CREATE POLICY "Users can delete own reservations" ON "Reservations"
  FOR DELETE
  USING ("Email" = auth.jwt() ->> 'email');

-- ============================================================================
-- STEP 4: Create RLS Policies for Public Tables (MenuItems)
-- ============================================================================

-- Policy 8: Public can view menu items (read-only)
CREATE POLICY "Public can view menu items" ON "MenuItems"
  FOR SELECT
  USING (true);

-- ============================================================================
-- STEP 5: Create RLS Policies for Public Tables (Chefs)
-- ============================================================================

-- Policy 9: Public can view chefs (read-only)
CREATE POLICY "Public can view chefs" ON "Chefs"
  FOR SELECT
  USING (true);

-- ============================================================================
-- STEP 6: Create RLS Policies for Public Tables (Reviews)
-- ============================================================================

-- Policy 10: Public can view reviews (read-only)
CREATE POLICY "Public can view reviews" ON "Reviews"
  FOR SELECT
  USING (true);

-- Policy 11: Public can create reviews
CREATE POLICY "Public can create reviews" ON "Reviews"
  FOR INSERT
  WITH CHECK (true);

-- ============================================================================
-- STEP 7: Create RLS Policies for ContactInquiries Table
-- ============================================================================

-- Policy 12: Public can create contact inquiries
CREATE POLICY "Public can create inquiries" ON "ContactInquiries"
  FOR INSERT
  WITH CHECK (true);

-- ============================================================================
-- STEP 8: Create RLS Policies for OrderItems Table (Derived from Orders)
-- ============================================================================

-- Policy 13: Users can view order items for their own orders
-- This assumes OrderItems has a reference to Orders (e.g., OrderId foreign key)
CREATE POLICY "Users can view own order items" ON "OrderItems"
  FOR SELECT
  USING (EXISTS (
    SELECT 1 FROM "Orders" o 
    WHERE o."OrderId" = "OrderItems"."OrderId" 
    AND o."Email" = auth.jwt() ->> 'email'
  ));

-- ============================================================================
-- STEP 9: Verification - Query all RLS Policies
-- ============================================================================

-- Display all created policies
SELECT 
  schemaname,
  tablename,
  policyname,
  permissive,
  cmd,
  qual as select_condition,
  with_check as insert_update_condition,
  roles
FROM pg_policies
WHERE schemaname = 'public'
  AND tablename IN ('Orders', 'Reservations', 'MenuItems', 'Chefs', 'Reviews', 'ContactInquiries', 'OrderItems')
ORDER BY tablename, policyname;

-- ============================================================================
-- STEP 10: Summary Report
-- ============================================================================

-- Count RLS policies per table
SELECT 
  tablename,
  COUNT(*) as policy_count
FROM pg_policies
WHERE schemaname = 'public'
  AND tablename IN ('Orders', 'Reservations', 'MenuItems', 'Chefs', 'Reviews', 'ContactInquiries', 'OrderItems')
GROUP BY tablename
ORDER BY tablename;

-- Total policies created
SELECT 
  COUNT(*) as total_policies
FROM pg_policies
WHERE schemaname = 'public'
  AND tablename IN ('Orders', 'Reservations', 'MenuItems', 'Chefs', 'Reviews', 'ContactInquiries', 'OrderItems');
