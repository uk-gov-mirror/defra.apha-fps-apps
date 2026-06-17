DO $$ 
DECLARE 
    row record;
    max_val bigint;
    seq_name text;
BEGIN
    RAISE NOTICE 'Starting post-migration identity sequence synchronization...';
    
    FOR row IN 
        SELECT 
            table_schema, 
            table_name, 
            column_name
        FROM 
            information_schema.columns 
        WHERE 
            is_identity = 'YES' 
            AND table_schema NOT IN ('pg_catalog', 'information_schema')
    LOOP
        -- Get the internal sequence name
        seq_name := pg_get_serial_sequence(quote_ident(row.table_schema) || '.' || quote_ident(row.table_name), row.column_name);
        
        IF seq_name IS NOT NULL THEN
            -- Get current max value
            EXECUTE format('SELECT MAX(%I) FROM %I.%I', row.column_name, row.table_schema, row.table_name) INTO max_val;
            
            -- Set the sequence to Max + 1 (or 1 if table is empty)
            EXECUTE format('SELECT setval(%L, %s, false)', seq_name, COALESCE(max_val, 0) + 1);
            
            RAISE NOTICE 'Table: %.%, Column: % | Sequence reset to %', 
                row.table_schema, row.table_name, row.column_name, COALESCE(max_val, 0) + 1;
        ELSE
            RAISE WARNING 'Table: %.%, Column: % | No sequence found.', 
                row.table_schema, row.table_name, row.column_name;
        END IF;
    END LOOP;
    
    RAISE NOTICE 'Synchronization complete.';
END $$;
