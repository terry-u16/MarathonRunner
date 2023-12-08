# MarathonRunner

## Rustコンパイラのデプロイ

1. `gcloud auth login`
2. `gcloud auth configure-docker asia-northeast1-docker.pkg.dev`
3. `cd MarathonRunner.CloudCompiler.Rust`
4. 必要に応じて `rust/Cargo.toml` を編集
5. `Publish-CloudRun.ps1`
