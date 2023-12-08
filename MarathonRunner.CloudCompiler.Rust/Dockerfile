#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM rust:1.70 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["MarathonRunner.CloudCompiler.Rust/MarathonRunner.CloudCompiler.Rust.csproj", "MarathonRunner.CloudCompiler.Rust/"]
RUN dotnet restore "MarathonRunner.CloudCompiler.Rust/MarathonRunner.CloudCompiler.Rust.csproj"
COPY . .
WORKDIR "/src/MarathonRunner.CloudCompiler.Rust"
RUN dotnet build "MarathonRunner.CloudCompiler.Rust.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MarathonRunner.CloudCompiler.Rust.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
RUN apt-get update && apt-get install sudo
RUN wget https://packages.microsoft.com/config/debian/11/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
    && sudo dpkg -i packages-microsoft-prod.deb \
    && rm packages-microsoft-prod.deb
RUN sudo apt-get update && sudo apt-get install -y aspnetcore-runtime-6.0
COPY MarathonRunner.CloudCompiler.Rust/rust/ ./work/temp
RUN cd ./work/temp && cargo build --release --bin main
RUN cp ./work/temp/target ./work/target -r \
    && cp ./work/temp/Cargo.toml ./work/Cargo.toml \
    && cp ./work/temp/Cargo.lock ./work/Cargo.lock
RUN rm -r ./work/temp
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TerryU16.MarathonRunner.CloudCompiler.Rust.dll"]