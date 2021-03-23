docker run -it --rm -p 2181:2181 -p 9092:9092 -e ADVERTISED_HOST=host.docker.internal -e NUM_PARTITIONS=10 johnnypark/kafka-zookeeper:2.4.0
