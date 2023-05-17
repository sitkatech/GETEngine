FROM r-base:latest

# the libraries needed by the program should be installed
RUN apt-get update
RUN apt-get -y install r-cran-readr
RUN apt-get -y install r-cran-httr

## set the enviroment here and uncomment the code for local testing
## do not need this when pushing the image to Azure
# ENV SOURCE_FOLDER=input
# ENV ANALYSIS_URL=http://1890b364.ngrok.io/api/RunAnalysis

ADD . /input 

# include files here that are part of the input process and always constant (i.e. not uploaded by users)
# the files are not included with this because we don't want to commit unnecessary files to source code
ADD ./AdjustCOHYSTrch.R ./
ADD ./COHYSTrecharge.RCH ./

# this is the entrypoint for the script to be run
CMD ["Rscript", "./AdjustCOHYSTrch.R"]