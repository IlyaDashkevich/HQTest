## Installation 

### 1. Clone the Repository
Склонируйте репозиторий с GitHub:
```sh
git clone https://github.com/IlyaDashkevich/HQTest.git
```

### 2. Install Dependencies
Откройте **терминал** (`Ctrl + ~` в VS Code или `Alt + F12` в Rider) и выполните команду:
```sh
dotnet restore
```

### 3. Configure API Keys
Откройте файл `appsettings.json` и вставьте свои API-ключи Binance:
```json
{
  "Binance": {
    "ApiKey": "your_binance_api_key",
    "ApiSecret": "your_binance_api_secret"
  }
}
```

### 4. Run the Application
Запустите сервер:
  ```sh
  dotnet run
```