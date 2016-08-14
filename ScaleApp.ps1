Connect-ServiceFabricCluster 

Update-ServiceFabricService -ServiceName fabric:/Voting/VotingService -Stateless -InstanceCount 3 -Force