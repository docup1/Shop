import { OrderFormController } from '../controllers/OrderFormController'

export function CreateOrderPage(): React.JSX.Element {
  return (
    <div className="page">
      <OrderFormController />
    </div>
  )
}
