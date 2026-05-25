### Учебный проект - микросервисное приложение (Transaction Outbox, Api Gateway, EDD)
Приложение из 4 микросервисов: 

- OrderService - для регистрации заявок на покупку товара, сообщения о регистрации заявок пишутся в outbox-таблицу, Worker SDK поллер отправляет их в Kafka
- InventoryService - фиксирует списание товара со склада - отправляет сообщение в сервис уведомлений
- NotificationService - отправляет уведомление на email покупателя
- AuthService - сервис-единая точка входа, работает в связке с Nginx (Api gateway)

Используемые технологии
- .NET 8 (ASP.NET Core Web API + Worker SDK)
- Postgresql 16
- Kafka
- Docker
- JWT auth
- Nginx
