import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from '../i18n/context'
import { ordersApi } from '../services/orders'
import { ApiError } from '../services/client'
import { OrderForm, type OrderFormState } from '../views/OrderForm'

type OrderFieldError = Partial<Record<keyof OrderFormState, string | undefined>>

const initialForm: OrderFormState = {
  senderCity: '',
  recipientCity: '',
  senderAddress: '',
  recipientAddress: '',
  weight: '',
}

export function OrderFormController(): React.JSX.Element {
  const { t } = useTranslation()
  const navigate = useNavigate()

  const [form, setForm] = useState<OrderFormState>(initialForm)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [fieldErrors, setFieldErrors] = useState<OrderFieldError>({})

  function onFieldChange(field: keyof OrderFormState, value: string): void {
    setForm((current) => ({ ...current, [field]: value }))
  }

  async function handleSubmit(): Promise<void> {
    const errors: OrderFieldError = {}
    const weight = Number(form.weight)
    if (!form.senderCity.trim()) errors.senderCity = t('orderForm.errors.senderCityRequired')
    if (!form.recipientCity.trim()) errors.recipientCity = t('orderForm.errors.recipientCityRequired')
    if (!form.senderAddress.trim()) errors.senderAddress = t('orderForm.errors.senderAddressRequired')
    if (!form.recipientAddress.trim()) errors.recipientAddress = t('orderForm.errors.recipientAddressRequired')
    if (!form.weight.trim() || !Number.isFinite(weight) || weight <= 0) {
      errors.weight = t('orderForm.errors.weightPositive')
    }
    setFieldErrors(errors)
    if (Object.keys(errors).length > 0) return

    setLoading(true)
    setError(null)
    try {
      const order = await ordersApi.create({
        senderCity: form.senderCity.trim(),
        recipientCity: form.recipientCity.trim(),
        senderAddress: form.senderAddress.trim(),
        recipientAddress: form.recipientAddress.trim(),
        weight,
      })
      navigate(`/orders/${order.id}`)
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message)
      } else {
        setError(t('common.error'))
      }
    } finally {
      setLoading(false)
    }
  }

  return (
    <OrderForm
      form={form}
      onFieldChange={onFieldChange}
      onSubmit={() => void handleSubmit()}
      error={error}
      fieldErrors={fieldErrors}
      loading={loading}
    />
  )
}
