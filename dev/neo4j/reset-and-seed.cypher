// DESTRUCTIVE: development graph only. Runs on every container start.
MATCH (n) DETACH DELETE n;

CREATE CONSTRAINT account_id_unique IF NOT EXISTS
FOR (a:Account) REQUIRE a.accountId IS UNIQUE;

CREATE CONSTRAINT device_id_unique IF NOT EXISTS
FOR (d:Device) REQUIRE d.deviceId IS UNIQUE;

MERGE (a:Account {accountId: '22222222-2222-2222-2222-222222222222'})
MERGE (b:Account {accountId: '33333333-3333-3333-3333-333333333333'})
MERGE (c:Account {accountId: '44444444-4444-4444-4444-444444444444'})
MERGE (shared:Device {deviceId: 'synthetic-device-shared'})
MERGE (separate:Device {deviceId: 'synthetic-device-separate'})
MERGE (a)-[:USED_DEVICE]->(shared)
MERGE (b)-[:USED_DEVICE]->(shared)
MERGE (c)-[:USED_DEVICE]->(separate);

MERGE (bridge:Account {
    accountId: '33333333-3333-3333-3333-333333333333'
})
MERGE (indirect:Account {
    accountId: '55555555-5555-5555-5555-555555555555'
})
MERGE (device:Device {
    deviceId: 'synthetic-device-bridge'
})
MERGE (bridge)-[:USED_DEVICE]->(device)
MERGE (indirect)-[:USED_DEVICE]->(device);

CALL gds.graph.drop('fraud-device-graph', false)
YIELD graphName
RETURN graphName;

CALL gds.graph.project(
    'fraud-device-graph',
    ['Account', 'Device'],
    {
        USED_DEVICE: {
            orientation: 'UNDIRECTED'
        }
    }
)
YIELD graphName, nodeCount, relationshipCount
RETURN graphName, nodeCount, relationshipCount;