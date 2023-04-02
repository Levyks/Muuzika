<script lang="ts">
    import { HubConnectionBuilder } from '@microsoft/signalr';
    import {onMount} from "svelte";
    
    onMount(() => {
        let token ="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VybmFtZSI6IkxldnlrcyIsInJvb21Db2RlIjoiODQzNSIsIm5iZiI6MTY4MDM4OTYyOSwiZXhwIjoxNzEyMDEyMDI5LCJpYXQiOjE2ODAzODk2MjksImlzcyI6Imh0dHBzOi8vbXV1emlrYS5jb20vIiwiYXVkIjoiaHR0cHM6Ly9tdXV6aWthLmNvbS8ifQ.oM5FlK-DTYc9HF3jlHFtcFW_r2zKFwnUe4gsVmTdDEg";
        let connection = new HubConnectionBuilder()
            .withUrl("https://localhost:7245/hub?token="+token)
            .build();

        connection.on('syncRoom', (...args) => {
            console.log(`Received event 'syncRoom' with arguments:`, ...args);
        });

        (window as any).connection = connection;

        connection.start().then(() => console.log('connected'));
    });
</script>

<h1>Welcome to SvelteKit</h1>
<p>Visit <a href="https://kit.svelte.dev">kit.svelte.dev</a> to read the documentation</p>
