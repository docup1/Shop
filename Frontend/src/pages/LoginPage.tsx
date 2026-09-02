import { AuthController } from '../controllers/AuthController'

export function LoginPage(): React.JSX.Element {
  return (
    <div className="page">
      <AuthController initialMode="login" />
    </div>
  )
}
