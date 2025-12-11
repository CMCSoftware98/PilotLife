import 'vuetify/styles'
import '@mdi/font/css/materialdesignicons.css'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'

const pilotLifeTheme = {
  dark: false,
  colors: {
    background: '#f8fafc',
    surface: '#ffffff',
    'surface-variant': '#f1f5f9',
    primary: '#0ea5e9',
    secondary: '#06b6d4',
    tertiary: '#8b5cf6',
    accent: '#8b5cf6',
    error: '#ef4444',
    warning: '#f97316',
    success: '#10b981',
    info: '#0ea5e9',
    'on-background': '#0f172a',
    'on-surface': '#0f172a',
  },
  variables: {
    'border-color': '#e2e8f0',
    'border-opacity': 1,
  }
}

export default createVuetify({
  components,
  directives,
  theme: {
    defaultTheme: 'pilotLifeTheme',
    themes: {
      pilotLifeTheme,
    },
  },
  defaults: {
    VBtn: {
      rounded: 'lg',
      fontWeight: '600',
    },
    VTextField: {
      variant: 'solo-filled',
      rounded: 'lg',
      density: 'comfortable',
    },
    VCard: {
      rounded: 'xl',
    },
  },
})
