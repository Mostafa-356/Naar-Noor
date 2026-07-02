-- Phase 2 Task 2.5: Storage Bucket Policies
-- Naar-Noor API - Supabase Storage
-- Purpose: Configure RLS policies for chef-images and menu-item-images buckets
-- Date: June 28, 2026

-- ============================================================================
-- IMPORTANT: Storage policies must be created via Supabase Dashboard
-- ============================================================================
-- Due to permission restrictions, storage.objects table modifications
-- must be done through Supabase Storage UI or by account owner.
-- 
-- Alternative: Use Supabase Dashboard → Storage → Policies
-- 
-- The policies below are for reference/documentation purposes.
-- ============================================================================
-- STEP 1: Storage Policies (Create via Supabase Dashboard)
-- ============================================================================

-- ============================================================================
-- MANUAL SETUP: Create Policies in Supabase Storage Dashboard
-- ============================================================================
-- 
-- Go to: Supabase Dashboard → Storage → Policies
-- 
-- For bucket: chef-images
-- ---------------------------
-- Policy 1: Public Read
--   Target roles: Public
--   Allowed operations: SELECT
--   USING: true
--
-- Policy 2: Authenticated Upload
--   Target roles: Authenticated
--   Allowed operations: INSERT
--   USING: auth.role() = 'authenticated'
--
-- Policy 3: Authenticated Delete
--   Target roles: Authenticated  
--   Allowed operations: DELETE
--   USING: auth.role() = 'authenticated'
--
-- Policy 4: Authenticated Update
--   Target roles: Authenticated
--   Allowed operations: UPDATE
--   USING: auth.role() = 'authenticated'
--
-- For bucket: menu-item-images
-- ---------------------------
-- Same 4 policies as above for this bucket
--
-- ============================================================================
-- VERIFICATION (After creating policies manually)
-- ============================================================================
-- Run this query to verify policies exist:

SELECT 
  policyname,
  cmd,
  roles
FROM pg_policies
WHERE schemaname = 'storage'
  AND tablename = 'objects'
ORDER BY policyname;

