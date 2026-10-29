#!/bin/bash
set -eu

# This marker must be cleared even when restarting the same container.
rm -f /tmp/fraud-seed-ready

initialize_graph() {
    for attempt in $(seq 1 120); do
        if cypher-shell -a bolt://localhost:7687 -u neo4j -p "$SEED_PASSWORD" 'RETURN 1;' >/dev/null 2>&1; then
            echo 'Development reset: deleting graph data and loading synthetic seeds.' >&2
            if cypher-shell -a bolt://localhost:7687 -u neo4j -p "$SEED_PASSWORD" --file /seed/reset-and-seed.cypher; then
                touch /tmp/fraud-seed-ready
                echo 'Synthetic graph ready.' >&2
            else
                echo 'Graph seeding failed; container will remain unhealthy.' >&2
            fi
            return
        fi
        sleep 2
    done
    echo 'Neo4j did not become available for seeding; container will remain unhealthy.' >&2
}

initialize_graph &
exec /startup/docker-entrypoint.sh "$@"
