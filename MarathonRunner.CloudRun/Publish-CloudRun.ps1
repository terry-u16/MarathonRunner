$projectName = "terry-u16-marathon-runner"
$repositoryName = "docker"
$containerName = "marathon-runner"
$containerFullName = "asia-northeast1-docker.pkg.dev/{0}/{1}/{2}" -f $projectName, $repositoryName, $containerName
docker build -t $containerFullName -f .\Dockerfile ..
docker push $containerFullName
gcloud run deploy --project $projectName --image $containerFullName --concurrency 1 --cpu 1 --memory 512Mi --max-instances 1000 --region asia-northeast1