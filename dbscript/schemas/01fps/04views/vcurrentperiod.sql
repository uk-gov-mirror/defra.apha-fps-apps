-- View: fps.vcurrentperiod

CREATE OR REPLACE VIEW fps.vcurrentperiod AS
 SELECT periodname,
    fpsyear
   FROM fps.tblperiod
  WHERE (endperiod = ( SELECT max(tblperiod_1.endperiod) AS maxendperiod
           FROM fps.tblperiod tblperiod_1
          GROUP BY tblperiod_1.finalsummariesrun
         HAVING (tblperiod_1.finalsummariesrun = '-1'::integer)));
