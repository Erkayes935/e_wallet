# EWalletAPI

API sederhana e-wallet berbasis ASP.NET Core dengan fitur autentikasi JWT, top up saldo, transfer antar user, cek saldo, dan riwayat transaksi.

## Fitur

- Register user baru
- Login dan generate JWT token
- Auto create wallet saat register
- Top up saldo wallet
- Transfer saldo antar user
- Cek saldo wallet
- Lihat daftar transaksi / statement
- Dashboard sederhana via Razor Page (`/Dashboard`)

## Tech Stack

- .NET 9 (`net9.0`)
- ASP.NET Core Web API + Razor Pages
- Entity Framework Core 9
- PostgreSQL (Npgsql)
- JWT Bearer Authentication
- BCrypt untuk hash password
- Swagger (environment Development)

## Struktur Project

```text
Controllers/
  AuthController.cs
  WalletController.cs
Data/
  AppDbContext.cs
DTOs/
  RegisterRequest.cs
  LoginRequest.cs
  TopUpRequest.cs
  TransferRequest.cs
Models/
  User.cs
  Wallet.cs
  Transaction.cs
Services/
  AuthService.cs
Pages/
  Dashboard.cshtml
  Dashboard.cshtml.cs
Migrations/
Program.cs
appsettings.json
```

## Konfigurasi

Connection string dan JWT ada di `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=ewalletdb;Username=postgres;Password=postgres"
},
"Jwt": {
  "Key": "THIS_IS_SECRET_KEY_FOR_DEV_ONLY",
  "Issuer": "EWalletAPI",
  "Audience": "EWalletUsers"
}
```

Sebelum run, sesuaikan nilainya dengan environment kamu.

## Menjalankan Project

1. Restore dependency:

```bash
dotnet restore
```

2. Install EF CLI (sekali saja, jika belum ada):

```bash
dotnet tool install --global dotnet-ef
```

3. Apply migration ke database:

```bash
dotnet ef database update
```

4. Jalankan aplikasi:

```bash
dotnet run
```

Default URL dari `launchSettings.json`:
- `http://localhost:5082`
- `https://localhost:7080`

Swagger tersedia di:
- `http://localhost:5082/swagger` (Development)

## Auth Flow

1. Register user: `POST /auth/register`
2. Login: `POST /auth/login`
3. Simpan token JWT dari response login
4. Kirim token di header endpoint wallet:

```http
Authorization: Bearer <JWT_TOKEN>
```

## Endpoint API

### Auth

#### `POST /auth/register`
Body:

```json
{
  "email": "user1@mail.com",
  "password": "password123"
}
```

Response sukses:
- `200 OK` -> `"User registered"`

#### `POST /auth/login`
Body:

```json
{
  "email": "user1@mail.com",
  "password": "password123"
}
```

Response sukses:

```json
{
  "token": "<jwt_token>"
}
```

### Wallet (butuh JWT)

#### `POST /wallet/topup`
Body:

```json
{
  "amount": 100000
}
```

Response:

```json
{
  "balance": 100000
}
```

#### `POST /wallet/transfer`
Body:

```json
{
  "toUserId": 2,
  "amount": 25000
}
```

Response sukses:
- `200 OK` -> `"Transfer success"`

#### `GET /wallet/balance`
Response:

```json
{
  "balance": 75000
}
```

#### `GET /wallet/transactions`
Response (contoh):

```json
[
  {
    "id": 10,
    "type": "Transfer",
    "amount": 25000,
    "createdAt": "2026-03-04T06:00:00Z"
  }
]
```

#### `GET /wallet/statement`
Response (contoh):

```json
{
  "balance": 75000,
  "transactions": [
    {
      "type": "TopUp",
      "amount": 100000,
      "date": "2026-03-04T05:30:00Z",
      "fromWallet": null,
      "toWallet": 1
    },
    {
      "type": "Transfer",
      "amount": 25000,
      "date": "2026-03-04T06:00:00Z",
      "fromWallet": 1,
      "toWallet": 2
    }
  ]
}
```

## Dashboard UI

Project punya halaman dashboard sederhana di:

- `GET /Dashboard`

Fungsi dashboard:
- Input JWT token manual
- Tombol cek saldo (`/wallet/balance`)
- Tombol lihat riwayat (`/wallet/statement`)

## Catatan Penting

- Endpoint wallet wajib token JWT valid.
- Wallet dibuat otomatis saat register.
- Transfer dijalankan dalam database transaction (`BeginTransactionAsync`) untuk menjaga konsistensi saldo.
- Aplikasi memanggil `UseHttpsRedirection()`, jadi request HTTP bisa redirect ke HTTPS.

## Testing Cepat (cURL)

Register:

```bash
curl -X POST http://localhost:5082/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"user1@mail.com","password":"password123"}'
```

Login:

```bash
curl -X POST http://localhost:5082/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user1@mail.com","password":"password123"}'
```

Top up (ganti `<TOKEN>`):

```bash
curl -X POST http://localhost:5082/wallet/topup \
  -H "Authorization: Bearer <TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{"amount":100000}'
```
