# Full-Stack Extension of Containerized E-Commerce Microservices System using Blazor

This repository contains the original containerized e-commerce microservices backend and the final full-stack extension with a Blazor frontend.

## Project Description

This project presents a full-stack extension of a containerized e-commerce microservices system by introducing a Blazor-based frontend on top of an existing API Gateway and backend service architecture. The system demonstrates end-to-end integration across multiple independently deployed services, including product, customer, order, and payment management, while preserving the microservices design principles of loose coupling, service isolation, and gateway-mediated communication. The frontend provides a simple but functional user interface for common business operations, and the entire system is designed to run consistently through Docker Compose for straightforward local deployment and demonstration.

## Final Project Module: Blazor Frontend Extension

The final project extends the existing backend system with a basic Blazor WebAssembly frontend. The frontend is deployed as a separate container and communicates only through the API Gateway.

### Frontend Features

- View all products and add a product
- View customers and add a customer
- View orders and create an order
- Cancel an order from the UI

### Architecture Summary

- Frontend: Blazor WebAssembly served from a separate container
- API Gateway: Ocelot-based gateway for all frontend API calls
- Backend services: Product, Customer, Order, and Payment services
- Messaging: RabbitMQ for existing event-driven order/product behavior

### Running the Full Stack

From the repository root:

```bash
docker compose up --build
```

### Service Entry Points

- Frontend UI: http://localhost:7010
- API Gateway: http://localhost:7001
- Gateway API routes: `/gateway/products`, `/gateway/customers`, `/gateway/orders`

### Frontend-to-Gateway Communication

The frontend uses `HttpClient` with the API Gateway base address and does not call backend services directly. All requests go through the gateway, which then forwards traffic to the corresponding microservices.

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

For the final project extension:

- Frontend UI: http://localhost:7010
- API Gateway: http://localhost:7001

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