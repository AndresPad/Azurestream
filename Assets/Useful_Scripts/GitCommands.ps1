#
# GitCommands.ps1
#
--In myWebApp, initialize a new Git repository:
    git init

--Open the Command Prompt and create a new working folder:
    git config --global user.name "John Doe"
    git config --global user.email "John Doe@azurestream.io"

--Create a new ASP.NET core application. 
--The new command offers a collection of switches that can be used for language, authentication, and framework selection (more details can be found on Microsoft docs: https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-new?tabs=netcore2x):
    mkdir azurestream

    dotnet new sln -n MyApp
    dotnet new mvc -n MyApp.Web
    dotnet new mstest -n MyApp.Test
    dotnet sln add MyApp.Web\MyApp.Web.csproj
    dotnet sln add MyApp.Test\MyApp.Test.csproj

--Launch Visual Studio Code in the context of the current working folder:
    code .
    git init

dotnet build
dotnet run

--With these commands, you have created a new branch, checked it out. The --list keyword shows you a list of all branches in your repository. The green colour represents the branch that's currently checked out
    git status
    git branch --list
    git branch feature-devops-home-page
    git checkout feature-devops-home-page
    git branch --list

--In the context of the git repository execute the following commands… These commands will stage the
--changes in the branch and then commit them.
    git status
    git add .
    git commit -m "updated welcome page"
    git status

--In order to merge the changes from the feature-devops-home-page into master, run the following commands in the context of the git repository.
    git checkout master
    git merge feature-devops-home-page

--To see the history of changes in the master branch run the command git log -v
    git log -v
--To investigate the actual changes in the commit, you can run the command git log -p
    git log -p

--Git makes it really easy to backout changes, following our example, if you wanted to take out the changes made to the welcome page, this can be done by hard resetting the master branch to a previous version of the commit using the command below
    git reset --hard 5d2441f0be4f1e4ca1f8f83b56dee31251367adc
    
--Run the below command to delete the feature branch
    git branch --delete feature-devops-home-page

az --version
az extension add --name azure-devops
az devops -h
az devops login --org https://dev.azure.com/nodestreamio
az devops configure --defaults organization=https://dev.azure.com/nodestreamio

az devops login --org https://dev.azure.com/geeks
az devops configure --defaults organization=https://dev.azure.com/geeks
az devops project list -o table

az pipelines list -p partsunlimited -o table
az pipelines build definition show --id 96 -p partsunlimited


git-tf clone --deep http://myOldAzure DevOps ServerServer/Azure DevOps
Server/DefaultCollection $/OldTeamProject/App2BeMigrated C:\migrated\App2BeMigrated

git filter-branch -f --msg-filter "sed 's/^git-Azure DevOps Serverid:.*;C\
([0-9]*\)$/Changeset:\1/g'" -- --all

git push -u origin –all

git filter-branch -f --commit-filter "
if [ "$GIT_COMMITTER_NAME" = "<old Azure DevOps Server user>" ];
then GIT_COMMITTER_NAME="<new name>";
GIT_AUTHOR_NAME="<new name>"; GIT_COMMITTER_
EMAIL="<new - email>"; GIT_AUTHOR_
EMAIL="<new - email>";
git commit-tree "$@";
else
git commit-tree "$@";
fi" HEAD

git tfs list-remote-branches https://dev.azure.com/Geeks/