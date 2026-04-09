# Containerized E-Commerce Microservices Backend

Midterm project built with ASP.NET Core and SQLite.

## Services

- Customer Service: manages customers
- Product Service: manages products
- Order Service: creates orders and validates customer/product data
- Payment Service: creates payments and validates order amount

Each service is an independent Web API with its own SQLite database and EF Core migrations.

## Tech Stack

- .NET 9 Web API
- Entity Framework Core 9 + SQLite
- Docker + Docker Compose
- Swagger

## Run with Docker

From the repository root:

```bash
docker compose up --build
```

Service URLs:

- Customer Service: http://localhost:5001/swagger
- Product Service: http://localhost:5002/swagger
- Order Service: http://localhost:5003/swagger
- Payment Service: http://localhost:5004/swagger

## Main API Routes

- Customers: `/api/customers`
- Products: `/api/products`
- Orders: `/api/orders`
- Payments: `/api/payment`

## Quick End-to-End Flow (example)

1. Create a customer (Customer Service)
2. Create a product (Product Service)
3. Create an order with `customerId`, `productId`, and `quantity` (Order Service)
4. Create a payment with matching `orderId` and `amount` (Payment Service)

## Notes

- SQLite files are persisted through mounted `Data` folders configured in `docker-compose.yml`.
- Services communicate internally through Docker network hostnames (for example, `customerservice`, `productservice`, `orderservice`).

## Project B: API Gateway + Event-Driven Extensions

This branch extends the original microservices project with API Gateway integration and RabbitMQ-based event communication.

### New Features

- API Gateway built with Ocelot
- Single entry point for client requests
- Route mapping for all services through the gateway
- Aggregated endpoint for order + customer + product data
- RabbitMQ event flow for `order_created` and `order_cancelled`
- Stock decreases on order creation and restores on order cancellation
- DTO-based request/response models across services
- Internal services are not exposed directly; requests go through the gateway

### Run Project B

From the repository root:

```bash
docker compose up --build
```

Gateway URL:

- API Gateway: http://localhost:7001/swagger

### Gateway Examples

- Customers: `/gateway/customers`
- Products: `/gateway/products`
- Orders: `/gateway/orders`
- Cancel order: `/gateway/orders/{id}/cancel`
- Payments: `/gateway/payment`
- Aggregate endpoint: `/aggregate/order-details/{orderId}`

### Event Flow

1. Create an order through the gateway.
2. `OrderService` publishes an `order_created` event to RabbitMQ.
3. `ProductService` consumes the event and decreases stock.
4. Cancel the order through the gateway.
5. `OrderService` publishes an `order_cancelled` event.
6. `ProductService` restores stock.

### How To Test (Project B)

Use API Gateway as the single entry point:

```bash
BASE=http://localhost:7001
```

Create a customer:

```bash
curl -s -X POST "$BASE/gateway/customers" \
	-H "Content-Type: application/json" \
	-d '{"name":"Alice","email":"alice@example.com"}'
```

Create a product:

```bash
curl -s -X POST "$BASE/gateway/products" \
	-H "Content-Type: application/json" \
	-d '{"name":"Keyboard","price":99.9,"stock":10}'
```

Create an order:

```bash
curl -s -X POST "$BASE/gateway/orders" \
	-H "Content-Type: application/json" \
	-d '{"customerId":1,"productId":1,"quantity":2}'
```

Check aggregate endpoint:

```bash
curl -s "$BASE/aggregate/order-details/1"
```

Cancel the order:

```bash
curl -s -X DELETE "$BASE/gateway/orders/1/cancel"
```

Check product stock (should be restored after cancel):

```bash
curl -s "$BASE/gateway/products/1"
```

Optional: check RabbitMQ queue counters:

```bash
docker exec rabbitmq rabbitmqctl list_queues name messages_ready messages_unacknowledged consumers
```