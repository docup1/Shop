import { createContext, useCallback, useContext, useMemo, type ReactNode } from 'react'
import { locale, type TranslationKey } from './locale'

type FlatLocale = {
  [K in TranslationKey]: string
}

const flat = flatten(locale) as FlatLocale

function flatten(
  obj: unknown,
  prefix = '',
  result: Record<string, string> = {},
): Record<string, string> {
  if (obj === null || typeof obj !== 'object') return result
  for (const [key, value] of Object.entries(obj)) {
    const path = prefix ? `${prefix}.${key}` : key
    if (typeof value === 'string') {
      result[path] = value
    } else {
      flatten(value, path, result)
    }
  }
  return result
}

const I18nContext = createContext<FlatLocale>(flat)

export function I18nProvider({ children }: { children: ReactNode }): React.JSX.Element {
  return <I18nContext.Provider value={flat}>{children}</I18nContext.Provider>
}

export function useTranslation(): { t: (key: TranslationKey) => string } {
  const dict = useContext(I18nContext)

  // t должен быть стабильной ссылкой между рендерами: если возвращать новый
  // объект/функцию каждый рендер, useCallback/useEffect с зависимостью [t] в
  // контроллерах будут пересоздаваться бесконечно и DDoS-ить бэкенд запросами.
  const t = useCallback((key: TranslationKey) => dict[key] ?? key, [dict])

  return useMemo(() => ({ t }), [t])
}
