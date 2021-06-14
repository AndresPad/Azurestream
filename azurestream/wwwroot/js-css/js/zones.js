function getVehiclesData() {

    // Simulate retrieval of vehicle data from the backend
    var data = {
        type: 'FeatureCollection',
        features: [
            {
                'type': 'Feature',
                'id': '1',
                'geometry': {
                    'type': 'Point',
                    'coordinates': [-111.9094154, 40.986327]
                },
                'properties': {
                    'vehicleType': 'truck',
                    'fleetNumber': '1984'
                }
            },
            {
                'type': 'Feature',
                'id': '2',
                'geometry': {
                    'type': 'Point',
                    'coordinates': [-111.899525, 40.985854]
                },
                'properties': {
                    'vehicleType': 'truck',
                    'fleetNumber': '2245'
                }
            },
            {
                'type': 'Feature',
                'id': '3',
                'geometry': {
                    'type': 'Point',
                    'coordinates': [-111.9014963, 40.9740979]
                },
                'properties': {
                    'vehicleType': 'truck',
                    'fleetNumber': '9988'
                }
            },
            {
                'type': 'Feature',
                'id': '4',
                'geometry': {
                    'type': 'Point',
                    'coordinates': [-111.9236773, 40.9785998]
                },
                'properties': {
                    'vehicleType': 'truck',
                    'fleetNumber': '8932'
                }
            },
            {
                'type': 'Feature',
                'id': '5',
                'geometry': {
                    'type': 'Point',
                    'coordinates': [-111.934460, 40.982843]
                },
                'properties': {
                    'vehicleType': 'excavator',
                    'fleetNumber': '8975'
                }
            },
            {
                'type': 'Feature',
                'id': '6',
                'geometry': {
                    'type': 'Point',
                    'coordinates': [-111.936638, 40.9829163]
                },
                'properties': {
                    'vehicleType': 'excavator',
                    'fleetNumber': '1342'
                }
            },
            {
                'type': 'Feature',
                'id': '9',
                'geometry': {
                    'type': 'Point',
                    'coordinates': [-111.9172021, 40.9792649]
                },
                'properties': {
                    'vehicleType': 'excavator',
                    'fleetNumber': '4231'
                }
            }
        ]
    };

    return data;
}