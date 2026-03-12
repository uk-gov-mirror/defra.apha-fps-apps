-- View: fps.qrytestspccostplan_xtab

CREATE OR REPLACE VIEW fps.qrytestspccostplan_xtab AS
SELECT testcode,
    fpsyear,
    sum(
        CASE profitcentre
            WHEN 'LabT'::text THEN (price)::numeric
            ELSE (0)::numeric
        END) AS labt,
    sum(
        CASE profitcentre
            WHEN 'VSD GB'::text THEN (price)::numeric
            ELSE (0)::numeric
        END) AS vetr,
    sum(
        CASE profitcentre
            WHEN 'Viro'::text THEN (price)::numeric
            ELSE (0)::numeric
        END) AS viro
   FROM fps.tbltestrccost
  GROUP BY testcode, fpsyear;
