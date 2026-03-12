-- View: fps.vtbltestrequ

CREATE OR REPLACE VIEW fps.vtbltestrequ AS
SELECT buyer AS jobcode,
    testcode,
    norequired AS notests,
    unitprice AS testprice,
    datecreated,
    projectbuyercode,
    fpsyear
   FROM fps.tlkptestreqmt
  WHERE ((buyer)::text IN ( SELECT tlkpproject.parentproject
           FROM fps.tlkpproject
          WHERE ((tlkpproject.program)::text IN ( SELECT tlkpprogram.programno
                   FROM fps.tlkpprogram
                  WHERE ((tlkpprogram.programno)::text IN ( SELECT tbluser_program.programno
                           FROM fps.tbluser_program
                          WHERE (tbluser_program.user_id IN ( SELECT tblusers.user_id
                                   FROM fps.tblusers
                                  WHERE ((tblusers.dt2username)::text = CURRENT_USER)))))))));
