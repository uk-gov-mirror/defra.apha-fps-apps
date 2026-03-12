-- View: fps.vtestprojectmonthfinal

CREATE OR REPLACE VIEW fps.vtestprojectmonthfinal AS
 SELECT monthno,
    fpsyear,
    sum(costprofile) AS costprofile,
    sum(subcontracts) AS subcontracts,
    sum(animals) AS animals,
    sum(nonanimals) AS nonanimals,
    sum(timecosts) AS timecosts,
    sum(transfercosts) AS transfercosts,
    sum(totalcost) AS totalcost,
    sum(invoices) AS invoices,
    sum(coiw) AS coiw,
    sum(portsales) AS portsales,
    sum(cumcost) AS cumcost,
    sum(cumprofile) AS cumprofile,
    sum(sumofcostprofile) AS sumofcostprofile,
    sum(cuminvoices) AS cuminvoices,
    sum(cumcoiw) AS cumcoiw,
    sum(cumportsales) AS cumportsales,
    sum(mstonedue) AS mstonedue,
    sum(due__done) AS due__done,
    sum(ontime) AS ontime,
    sum(sumofmstonedue) AS sumofmstonedue,
    sum(sumofdue__done) AS sumofdue__done,
    sum(sumofontime) AS sumofontime
   FROM fps.projectmonthfinal
  GROUP BY monthno, fpsyear;
