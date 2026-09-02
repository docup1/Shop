import { useTranslation } from '../i18n/context'

export interface OrderFormState {
  senderCity: string
  recipientCity: string
  senderAddress: string
  recipientAddress: string
  weight: string
}

export type OrderFormErrors = Partial<Record<'senderCity' | 'recipientCity' | 'senderAddress' | 'recipientAddress' | 'weight', string>>

interface OrderFormProps {
  form: OrderFormState
  onFieldChange: (field: keyof OrderFormState, value: string) => void
  onSubmit: () => void
  error: string | null
  fieldErrors: OrderFormErrors
  loading: boolean
}

export function OrderForm({
  form,
  onFieldChange,
  onSubmit,
  error,
  fieldErrors,
  loading,
}: OrderFormProps): React.JSX.Element {
  const { t } = useTranslation()

  const fields: Array<{
    key: keyof OrderFormState
    label: string
    errorKey: keyof OrderFormErrors
    inputType?: string
  }> = [
    { key: 'senderCity', label: t('orderForm.senderCity'), errorKey: 'senderCity' },
    { key: 'recipientCity', label: t('orderForm.recipientCity'), errorKey: 'recipientCity' },
    { key: 'senderAddress', label: t('orderForm.senderAddress'), errorKey: 'senderAddress' },
    { key: 'recipientAddress', label: t('orderForm.recipientAddress'), errorKey: 'recipientAddress' },
    { key: 'weight', label: t('orderForm.weight'), errorKey: 'weight', inputType: 'number' },
  ]

  return (
    <form
      className="card order-form"
      onSubmit={(event) => {
        event.preventDefault()
        if (!loading) onSubmit()
      }}
      noValidate
    >
      <h2 className="order-form__title">{t('orderForm.title')}</h2>

      {fields.map((field) => (
        <label className="field" key={field.key}>
          <span className="field__label">{field.label}</span>
          <input
            className="field__input"
            type={field.inputType ?? 'text'}
            value={form[field.key]}
            onChange={(event) => onFieldChange(field.key, event.target.value)}
            required
          />
          {fieldErrors[field.errorKey] && (
            <span className="field__error">{fieldErrors[field.errorKey]}</span>
          )}
        </label>
      ))}

      {error && <p className="form-error" role="alert">{error}</p>}

      <button className="btn btn--primary" type="submit" disabled={loading}>
        {t('orderForm.submit')}
      </button>
    </form>
  )
}
