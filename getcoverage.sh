#!/bin/bash
dotnet test --collect:"XPlat Code Coverage"
coverage=$(find ./Tests/TestResults/* -type d -print0 | xargs -0 ls -tl | head -n 1 | tr -d : )
reports="-reports:$coverage/coverage.cobertura.xml"
reportgenerator $reports "-targetdir:./Tests/coveragereport" -reporttypes:Html