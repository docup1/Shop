export const locale = {
  common: {
    appName: 'Доставка',
    loading: 'Загрузка…',
    error: 'Ошибка',
    save: 'Сохранить',
    cancel: 'Отмена',
    back: 'Назад',
    logout: 'Выйти',
  },
  nav: {
    myOrders: 'Мои заказы',
    newOrder: 'Новый заказ',
    adminPanel: 'Админ-панель',
  },
  auth: {
    loginTitle: 'Вход',
    registerTitle: 'Регистрация',
    userName: 'Имя пользователя',
    password: 'Пароль',
    login: 'Войти',
    register: 'Зарегистрироваться',
    switchToLogin: 'Уже есть аккаунт? Войти',
    switchToRegister: 'Нет аккаунта? Зарегистрироваться',
    passwordHint: 'Минимум 8 символов',
    errors: {
      userNameRequired: 'Укажите имя пользователя',
      passwordTooShort: 'Пароль должен быть не короче 8 символов',
      default: 'Неправильные имя пользователя или пароль',
      taken: 'Это имя пользователя уже занято',
    },
  },
  orderForm: {
    title: 'Оформление заказа',
    senderCity: 'Город отправки',
    recipientCity: 'Город получения',
    senderAddress: 'Адрес отправки',
    recipientAddress: 'Адрес получения',
    weight: 'Вес (кг)',
    submit: 'Оформить заказ',
    errors: {
      senderCityRequired: 'Укажите город отправки',
      recipientCityRequired: 'Укажите город получения',
      senderAddressRequired: 'Укажите адрес отправки',
      recipientAddressRequired: 'Укажите адрес получения',
      weightPositive: 'Вес должен быть больше 0',
    },
  },
  orders: {
    title: 'Мои заказы',
    empty: 'Заказов пока нет',
    loadMore: 'Загрузить ещё',
    table: {
      senderCity: 'Откуда',
      recipientCity: 'Куда',
      weight: 'Вес',
      status: 'Статус',
      createdAt: 'Создан',
    },
  },
  orderDetails: {
    title: 'Заказ',
    notFound: 'Заказ не найден',
    createdAt: 'Создан',
  },
  status: {
    New: 'Новый',
    InProgress: 'В обработке',
    PickedUp: 'Забран',
    InTransit: 'В пути',
    OutForDelivery: 'У курьера',
    Delivered: 'Доставлен',
    Cancelled: 'Отменён',
  },
  admin: {
    title: 'Админ-панель',
    usersTab: 'Пользователи',
    ordersTab: 'Заказы',
    users: {
      userName: 'Имя пользователя',
      isAdmin: 'Администратор',
      id: 'ID',
      promote: 'Сделать админом',
      demote: 'Снять админа',
    },
    orders: {
      filterStatus: 'Фильтр по статусу',
    },
  },
} as const

export type TranslationKey =
  | 'common.appName'
  | 'common.loading'
  | 'common.error'
  | 'common.save'
  | 'common.cancel'
  | 'common.back'
  | 'common.logout'
  | 'nav.myOrders'
  | 'nav.newOrder'
  | 'nav.adminPanel'
  | 'auth.loginTitle'
  | 'auth.registerTitle'
  | 'auth.userName'
  | 'auth.password'
  | 'auth.login'
  | 'auth.register'
  | 'auth.switchToLogin'
  | 'auth.switchToRegister'
  | 'auth.passwordHint'
  | 'auth.errors.userNameRequired'
  | 'auth.errors.passwordTooShort'
  | 'auth.errors.default'
  | 'auth.errors.taken'
  | 'orderForm.title'
  | 'orderForm.senderCity'
  | 'orderForm.recipientCity'
  | 'orderForm.senderAddress'
  | 'orderForm.recipientAddress'
  | 'orderForm.weight'
  | 'orderForm.submit'
  | 'orderForm.errors.senderCityRequired'
  | 'orderForm.errors.recipientCityRequired'
  | 'orderForm.errors.senderAddressRequired'
  | 'orderForm.errors.recipientAddressRequired'
  | 'orderForm.errors.weightPositive'
  | 'orders.title'
  | 'orders.empty'
  | 'orders.loadMore'
  | 'orders.table.senderCity'
  | 'orders.table.recipientCity'
  | 'orders.table.weight'
  | 'orders.table.status'
  | 'orders.table.createdAt'
  | 'orderDetails.title'
  | 'orderDetails.notFound'
  | 'orderDetails.createdAt'
  | 'status.New'
  | 'status.InProgress'
  | 'status.PickedUp'
  | 'status.InTransit'
  | 'status.OutForDelivery'
  | 'status.Delivered'
  | 'status.Cancelled'
  | 'admin.title'
  | 'admin.usersTab'
  | 'admin.ordersTab'
  | 'admin.users.userName'
  | 'admin.users.isAdmin'
  | 'admin.users.id'
  | 'admin.users.promote'
  | 'admin.users.demote'
  | 'admin.orders.filterStatus'
