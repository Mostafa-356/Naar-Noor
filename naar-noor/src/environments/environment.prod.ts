export const environment = {
  production: true,
  apiUrl: (window as any)['env']?.apiUrl || 'https://naar-noor.runasp.net',
  supabaseUrl: (window as any)['env']?.supabaseUrl || 'https://your-project.supabase.co',
  supabaseAnonKey: (window as any)['env']?.supabaseAnonKey || ''
};
