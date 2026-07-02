-- Phase 2 PREREQUISITE: Create Database Tables
-- Naar-Noor API Database Schema
-- Date: June 28, 2026
-- Purpose: Create all required tables before applying RLS policies

-- ============================================================================
-- STEP 0: Create Tables (Run this FIRST before RLS script)
-- ============================================================================

-- Create Chefs table
CREATE TABLE IF NOT EXISTS "Chefs" (
    "ChefId" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "Name" text NOT NULL,
    "Bio" text,
    "ImageUrl" text,
    "CreatedAt" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Create MenuItems table
CREATE TABLE IF NOT EXISTS "MenuItems" (
    "MenuItemId" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "Name" text NOT NULL,
    "Description" text,
    "Price" decimal(10, 2) NOT NULL,
    "ImageUrl" text,
    "IsVegetarian" boolean NOT NULL DEFAULT false,
    "IsVegan" boolean NOT NULL DEFAULT false,
    "IsGlutenFree" boolean NOT NULL DEFAULT false,
    "ChefId" uuid,
    "CreatedAt" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_MenuItems_Chefs" FOREIGN KEY ("ChefId") REFERENCES "Chefs"("ChefId") ON DELETE SET NULL
);

-- Create Orders table
CREATE TABLE IF NOT EXISTS "Orders" (
    "OrderId" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "OrderNumber" text UNIQUE NOT NULL,
    "Email" text NOT NULL,
    "TotalPrice" decimal(10, 2) NOT NULL,
    "Status" text NOT NULL DEFAULT 'Pending',
    "CreatedAt" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Create OrderItems table
CREATE TABLE IF NOT EXISTS "OrderItems" (
    "OrderItemId" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "OrderId" uuid NOT NULL,
    "MenuItemId" uuid NOT NULL,
    "Quantity" integer NOT NULL DEFAULT 1,
    "UnitPrice" decimal(10, 2) NOT NULL,
    "CreatedAt" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_OrderItems_Orders" FOREIGN KEY ("OrderId") REFERENCES "Orders"("OrderId") ON DELETE CASCADE,
    CONSTRAINT "FK_OrderItems_MenuItems" FOREIGN KEY ("MenuItemId") REFERENCES "MenuItems"("MenuItemId") ON DELETE RESTRICT
);

-- Create Reservations table
CREATE TABLE IF NOT EXISTS "Reservations" (
    "ReservationId" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "Email" text NOT NULL,
    "ReservationNumber" text UNIQUE NOT NULL,
    "GuestCount" integer NOT NULL,
    "ReservationDate" timestamp NOT NULL,
    "SpecialRequests" text,
    "Status" text NOT NULL DEFAULT 'Pending',
    "CreatedAt" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Create Reviews table
CREATE TABLE IF NOT EXISTS "Reviews" (
    "ReviewId" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "MenuItemId" uuid,
    "ChefId" uuid,
    "Email" text NOT NULL,
    "Rating" integer NOT NULL,
    "Comment" text,
    "Status" text NOT NULL DEFAULT 'Pending',
    "CreatedAt" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_Reviews_MenuItems" FOREIGN KEY ("MenuItemId") REFERENCES "MenuItems"("MenuItemId") ON DELETE SET NULL,
    CONSTRAINT "FK_Reviews_Chefs" FOREIGN KEY ("ChefId") REFERENCES "Chefs"("ChefId") ON DELETE SET NULL
);

-- Create ContactInquiries table
CREATE TABLE IF NOT EXISTS "ContactInquiries" (
    "InquiryId" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "Name" text NOT NULL,
    "Email" text NOT NULL,
    "Subject" text NOT NULL,
    "Message" text NOT NULL,
    "Status" text NOT NULL DEFAULT 'New',
    "CreatedAt" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ============================================================================
-- Create Indexes for Performance
-- ============================================================================

CREATE INDEX IF NOT EXISTS "IX_Orders_Email" ON "Orders"("Email");
CREATE INDEX IF NOT EXISTS "IX_Orders_CreatedAt" ON "Orders"("CreatedAt");
CREATE INDEX IF NOT EXISTS "IX_Reservations_Email" ON "Reservations"("Email");
CREATE INDEX IF NOT EXISTS "IX_Reservations_ReservationDate" ON "Reservations"("ReservationDate");
CREATE INDEX IF NOT EXISTS "IX_MenuItems_ChefId" ON "MenuItems"("ChefId");
CREATE INDEX IF NOT EXISTS "IX_OrderItems_OrderId" ON "OrderItems"("OrderId");
CREATE INDEX IF NOT EXISTS "IX_OrderItems_MenuItemId" ON "OrderItems"("MenuItemId");
CREATE INDEX IF NOT EXISTS "IX_Reviews_MenuItemId" ON "Reviews"("MenuItemId");
CREATE INDEX IF NOT EXISTS "IX_Reviews_ChefId" ON "Reviews"("ChefId");

-- ============================================================================
-- Verification Queries
-- ============================================================================

-- Verify all tables created
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name IN ('Orders', 'Reservations', 'MenuItems', 'Chefs', 'Reviews', 'ContactInquiries', 'OrderItems')
ORDER BY table_name;

-- Count of tables created
SELECT COUNT(*) as tables_created
FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name IN ('Orders', 'Reservations', 'MenuItems', 'Chefs', 'Reviews', 'ContactInquiries', 'OrderItems');

-- ============================================================================
-- NEXT STEP
-- ============================================================================
-- After this script completes successfully (7 tables created):
-- 1. Run: Database/2_1_rls_implementation.sql (RLS policies)
-- 2. Run: Database/2_5_storage_policies.sql (Storage policies)
