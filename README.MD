1. Tạo project webapi
dotnet new webapi -n ten_project

2. Cài đặt thư viện cho entity frame work(data model)
dotnet add package Pomelo.EntityFrameworkCore.MySql ( Pomelo MySql Provider)
 dotnet add package Microsoft.EntityFrameworkCore.Tools
Nếu bị lỗi câu lệnh trên : dotnet tool install --global dotnet-ef


3. cai dat not ef :dotnet tool install --global dotnet-ef


4. Đồng bộ hóa vói Database (Tạo Migration)

dotnet ef migrations add InitialCreate

5. Cập nhật vao Database :
dotnet ef database update

kiểm tra version: dotnet ef --version

Db set đứng giữa App và database

Truy van query tren doi tuong class thi dung linq


dotnet ef migrations add AddEmailToCustomer


dotnet ef migrations remove


dotnet add package Hangfire.AspNetCore
dotnet add package Hangfire.SqlServer
