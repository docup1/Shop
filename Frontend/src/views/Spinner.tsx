import { useTranslation } from '../i18n/context'

export function Spinner(): React.JSX.Element {
  const { t } = useTranslation()
  return (
    <div className="spinner" role="status" aria-label={t('common.loading')}>
      <span className="spinner__ring" aria-hidden="true" />
      {t('common.loading')}
    </div>
  )
}
