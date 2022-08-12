$projectName = "terry-u16-marathon-runner"
$repositoryName = "docker"
$containerName = "rust-compiler"
$containerFullName = "asia-northeast1-docker.pkg.dev/{0}/{1}/{2}" -f $projectName, $repositoryName, $containerName
docker build -t $containerFullName -f .\Dockerfile ..
docker push $containerFullName
gcloud run deploy --project $projectName --image $containerFullName --concurrency 1 --max-instances 2 --region asia-northeast1