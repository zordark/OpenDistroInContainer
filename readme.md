# Small clouds for big projects or how Docker can help us with cloud services integration testing

This is demo code to Apollo Division knowledge snack

Code base contains demonstration how to use ElasticSearch AWS service (AWS OpenDisrto for ElasticSearch) within integration tests. There are 2 approaches: 
 - first, runs ES container through the docker-compose and then you can run tests against of it (see LambdaTests.cs)
 - second, runs ES container from the code through TestEnvironment.Docker framework and runs tests against of managed Docker container (see SearchWithManagedDockerTests.cs)

### Prerequisites: 
 - You need to install Docker desktop on the machine
 - You need to register and get your own API developer key for themoviedb.org, to be able fetch demo data from it (see https://www.themoviedb.org/settings/api)
 - You need to have MSSQL installed or you can run it from docker compose file in source code (docker-compose-mssql.yml)
 - You need to adjust configuration values (MSSQL server connection string and themoviedb.org API key) in appsettings.json file. You can create `appsettings.Development.json` with necessary values for that in test project.


### Links:
- https://github.com/Deffiss/testenvironment-docker
- https://www.themoviedb.org/
- https://developers.themoviedb.org/3/getting-started/introduction