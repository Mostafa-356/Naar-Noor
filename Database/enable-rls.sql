-- Phase 2 Task 2.1: Enable Row-Level Security (RLS) on all tables
-- Execute this script in Supabase SQL Editor

-- Step 1: Enable RLS on all tables
ALTER TABLE "Orders" ENABLE ROW LEVEL SECURITY;
ALTER TABLE "Reservations" ENABLE ROW LEVEL SECURITY;
ALTER TABLE "MenuItems" ENABLE ROW LEVEL SECURITY;
ALTER TABLE "Chefs" ENABLE ROW LEVEL SECURITY;
ALTER TABLE "Reviews" ENABLE ROW LEVEL SECURITY;
ALTER TABLE "ContactInquiries" ENABLE ROW LEVEL SECURITY;
ALTER TABLE "OrderItems" ENABLE ROW LEVEL SECURITY;

-- Verify RLS enabled
SELECT tablename, rowsecurity FROM pg_tables WHERE schemaname = 'public' AND tablename IN ('Orders', 'Reservations', 'MenuItems', 'Chefs', 'Reviews', 'ContactInquiries', 'OrderItems');

-- Step 2: Create RLS policies for Orders table
-- Users can see their own orders
CREATE POLICY "Users view own orders" ON "Orders"
  FOR SELECT
  USING (email = current_user_email());

-- Users can create orders
CREATE POLICY "Users create orders" ON "Orders"
  FOR INSERT
  WITH CHECK (true);

-- Users can update their own orders
CREATE POLICY "Users update own orders" ON "Orders"
  FOR UPDATE
  USING (email = current_user_email());

-- Step 3: Create RLS policies for Reservations table
-- Users can see their own reservations
CREATE POLICY "Users view own reservations" ON "Reservations"
  FOR SELECT
  USING (email = current_user_email());

-- Users can create reservations
CREATE POLICY "Users create reservations" ON "Reservations"
  FOR INSERT
  WITH CHECK (true);

-- Users can update their own reservations
CREATE POLICY "Users update own reservations" ON "Reservations"
  FOR UPDATE
  USING (email = current_user_email());

-- Users can delete their own reservations
CREATE POLICY "Users delete own reservations" ON "Reservations"
  FOR DELETE
  USING (email = current_user_email());

-- Step 4: Create RLS policies for public tables
-- Public read access to menu items
CREATE POLICY "Public view menu items" ON "MenuItems"
  FOR SELECT
  USING (true);

-- Public read access to chefs
CREATE POLICY "Public view chefs" ON "Chefs"
  FOR SELECT
  USING (true);

-- Public read access to reviews
CREATE POLICY "Public view reviews" ON "Reviews"
  FOR SELECT
  USING (true);

-- Public can create reviews
CREATE POLICY "Public create reviews" ON "Reviews"
  FOR INSERT
  WITH CHECK (true);

-- Public can create contact inquiries
CREATE POLICY "Public create inquiries" ON "ContactInquiries"
  FOR INSERT
  WITH CHECK (true);

-- Step 5: Verify all policies created
SELECT tablename, policyname FROM pg_policies WHERE tablename IN ('Orders', 'Reservations', 'MenuItems', 'Chefs', 'Reviews', 'ContactInquiries') ORDER BY tablename;
