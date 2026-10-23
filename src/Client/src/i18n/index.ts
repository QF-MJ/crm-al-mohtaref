import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'
import arCommon from './locales/ar/common.json'
import arAuth from './locales/ar/auth.json'
import enCommon from './locales/en/common.json'
import enAuth from './locales/en/auth.json'

void i18n.use(initReactI18next).init({
  resources: {
    ar: { common: arCommon, auth: arAuth },
    en: { common: enCommon, auth: enAuth },
  },
  lng: 'ar',
  fallbackLng: 'ar',
  defaultNS: 'common',
  interpolation: { escapeValue: false },
})

// الاتجاه يتبع اللغة تلقائياً
function applyDirection(lng: string) {
  document.documentElement.lang = lng
  document.documentElement.dir = lng === 'ar' ? 'rtl' : 'ltr'
}
applyDirection(i18n.language)
i18n.on('languageChanged', applyDirection)

export default i18n
