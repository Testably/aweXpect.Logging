# Contributor guide

## Pull requests

**Pull requests are welcome!**  
Please include a clear description of the changes you have made with your request; the title should follow
the [conventional commits](https://www.conventionalcommits.org/en/v1.0.0/) guideline.
All code should be covered by unit tests and comply with the coding guideline in this project.

### Technical expectations

As a framework for supporting unit testing, this project has a high standard for testing itself.  
In order to support this, static code analysis is performed
using [SonarCloud](https://sonarcloud.io/project/overview?id=Testably_aweXpect.Logging) with quality gate requiring to

- solve all issues reported by SonarCloud
- have a code coverage of > 90%

### Running the tests
The tests use [TUnit](https://tunit.dev/) on the Microsoft.Testing.Platform. Run all of them with `dotnet test`, or a
single project and target framework with
```
dotnet test --project Tests/aweXpect.Logging.Tests -f net10.0
```

The public API tests in `Tests/aweXpect.Logging.Api.Tests` fail when the public API changes. If the change is
intentional, accept it by running the explicit test `AcceptApiChanges`
```
dotnet test --project Tests/aweXpect.Logging.Api.Tests --treenode-filter "/*/*/ApiApprovalTests/AcceptApiChanges"
```
and commit the updated `*.awexpect-snapshot.txt` files.
