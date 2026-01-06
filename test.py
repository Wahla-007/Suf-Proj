import os

for item in [item.replace(".cs", "") for item in os.listdir("Models") if item.endswith(".cs") and item not in ["AppDbContext.cs", "ErrorViewModel.cs",]]:
    # print(f"Generating controller for model: {item}")
    print(f"dotnet aspnet-codegenerator controller --controllerName {item}Controller --model mess_management.Models.{item} --dataContext mess_management.Models.AppDbContext --useAsyncActions --useDefaultLayout --force --relativeFolderPath Controllers")
    # os.system(f"dotnet aspnet-codegenerator controller --controllerName {item}Controller --model mess_management.Models.{item} --dataContext mess_management.Models.AppDbContext --restWithNoViews --force --relativeFolderPath Controllers")