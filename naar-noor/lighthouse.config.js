module.exports = {
  ci: {
    collect: {
      url: ['http://localhost:5000/'],
      numberOfRuns: 3,
      settings: {
        chromePath: '/usr/bin/chromium-browser',
      },
    },
    upload: {
      target: 'temporary-public-storage',
    },
    assert: {
      preset: 'lighthouse:recommended',
      assertions: {
        // Performance
        'categories:performance': ['error', { minScore: 0.90 }],
        'largest-contentful-paint': ['error', { maxNumericValue: 2500 }],
        'cumulative-layout-shift': ['error', { maxNumericValue: 0.1 }],
        
        // Accessibility (WCAG 2.1 AA)
        'categories:accessibility': ['error', { minScore: 0.90 }],
        'color-contrast': ['error', { minScore: 0.95 }],
        'aria-hidden-body': 'error',
        'aria-required-attr': 'error',
        
        // Best Practices
        'categories:best-practices': ['error', { minScore: 0.90 }],
        
        // SEO
        'categories:seo': ['error', { minScore: 0.90 }],
        'meta-description': 'error',
        'viewport': 'error',
      },
    },
  },
};
