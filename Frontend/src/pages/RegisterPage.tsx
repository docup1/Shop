import { AuthController } from '../controllers/AuthController'

export function RegisterPage(): React.JSX.Element {
  return (
    <div className="page">
      <AuthController initialMode="register" />
    </div>
  )
}
