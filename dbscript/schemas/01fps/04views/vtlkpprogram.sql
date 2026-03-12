-- View: fps.vtlkpprogram

CREATE OR REPLACE VIEW fps.vtlkpprogram AS
 SELECT programno,
    programname,
    directorate,
    minim,
    sector_name,
    customer,
    target,
    manager,
    fpsyear
   FROM fps.tlkpprogram
  WHERE ((programno)::text IN ( SELECT tbluser_program.programno
           FROM fps.tbluser_program
          WHERE (tbluser_program.user_id IN ( SELECT tblusers.user_id
                   FROM fps.tblusers
                  WHERE ((tblusers.dt2username)::text = CURRENT_USER)))));
