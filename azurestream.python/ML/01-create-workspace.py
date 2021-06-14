# tutorial/01-create-workspace.py
from azureml.core import Workspace

ws = Workspace.create(name='YOURWORKSPACENAME', # provide a name for your workspace
                      subscription_id='SUBSCRIPTIONID', # provide your subscription ID
                      resource_group='AZU-MachineLearning-RG', # provide a resource group name
                      create_resource_group=True,
                      location='eastus') # For example: 'westeurope' or 'eastus2' or 'westus2' or 'southeastasia'.

# write out the workspace details to a configuration file: .azureml/config.json
ws.write_config(path='.azureml')