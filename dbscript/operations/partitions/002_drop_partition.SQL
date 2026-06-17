DO $$
DECLARE
    v_part_name TEXT;
    r           RECORD;
    p_schema    TEXT := current_setting('custom.p_schema', true);
    p_value     TEXT := current_setting('custom.p_value', true);
BEGIN
    -- Validate that inputs were passed correctly
    IF p_schema IS NULL OR p_value IS NULL THEN
        RAISE EXCEPTION 'Input parameters custom.p_schema or custom.p_value are missing!';
    END IF;

    -- Generate partition search pattern (e.g., '%_y2026')
    v_part_name := '%_y' || p_value;

    FOR r IN
        SELECT c.relname AS part_name
        FROM pg_inherits i
        JOIN pg_class c ON i.inhrelid = c.oid
        JOIN pg_class p ON i.inhparent = p.oid
        JOIN pg_namespace nm ON nm.oid = c.relnamespace
        WHERE nm.nspname = p_schema
          AND c.relname LIKE v_part_name
    LOOP
        RAISE NOTICE 'Dropping partition table: %.%', p_schema, r.part_name;
        
        -- Safely drop partition table
        EXECUTE format(
            'DROP TABLE %I.%I',
            p_schema,
            r.part_name
        );
    END LOOP;
END $$;
