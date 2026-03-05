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