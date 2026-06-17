DO $$
DECLARE
    v_part_name  TEXT;
    r            RECORD;
    p_schema     TEXT := current_setting('custom.p_schema', true);
    p_value      TEXT := current_setting('custom.p_value', true);
BEGIN
    -- Validate that inputs were passed correctly
    IF p_schema IS NULL OR p_value IS NULL THEN
        RAISE EXCEPTION 'Input parameters custom.p_schema or custom.p_value are missing!';
    END IF;

    FOR r IN
        SELECT
            n.nspname AS schema_name,
            c.relname AS table_name,
            a.attname AS column_name
        FROM pg_partitioned_table pt
        JOIN pg_class c ON c.oid = pt.partrelid
        JOIN pg_namespace n ON n.oid = c.relnamespace
        JOIN pg_attribute a 
            ON a.attrelid = c.oid 
           AND a.attnum = ANY(pt.partattrs)
        WHERE n.nspname = p_schema 
          AND pt.partstrat = 'l' -- List partitioning
          AND a.attname = 'year'
    LOOP
         -- Generate safe partition name
         v_part_name := r.table_name || '_y' || p_value;
             
         -- Skip if partition already exists in the specific schema
         IF EXISTS (
             SELECT 1 FROM pg_class c
             JOIN pg_namespace n ON n.oid = c.relnamespace
             WHERE c.relname = v_part_name 
               AND n.nspname = r.schema_name
         ) THEN
             RAISE NOTICE 'Partition %.% already exists, skipping.', r.schema_name, v_part_name;
         ELSE   
             -- Create partition
             EXECUTE format(
                 'CREATE TABLE %I.%I PARTITION OF %I.%I FOR VALUES IN (%L)',
                 r.schema_name,
                 v_part_name,
                 r.schema_name,
                 r.table_name,
                 p_value
             );
             RAISE NOTICE 'Partition %.% successfully created.', r.schema_name, v_part_name;
         END IF;
    END LOOP;
END $$;
