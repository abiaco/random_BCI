% Student Name:  put your name here
% Registration number: ????

% THIS IS NOT A GROUP ASSIGNMENT.
% DISCUSSION OF THE SOLUTION WITH OTHER STUDENTS, OR COLLABORATION
% IN ANY OTHER FORMS IS NOT ALLOWED.

% Matlab Version used:  versions 7 and beyond are accepted, but be sure to mention
%                  which version you used. (type 'ver' in matlab to find out)

% CE803 - HMI
% Assignment 2
% Partial solution by F. Sepulveda
% Last update: 1/03/2013


% This .m file has the hints and partial solution for the EEG signals
% with ear lobe referencing.
% Modify it accordingly for the EMG signals and for EEG without referencing
% (i.e., change the filters, remove the referencing step, consider the different sampling rates, etc., etc.).
%
% For simplicity, you should submit up to three .m files:
% one for EEG without ear referencing, one for EEG with referencing, 
% and one for EMG signals.  In that case, you should give the following
% names to the files:  A2eegnoref.m, A2eegref.m, and A2emg.m.
% 
% The steps to be followed are described in the comments below
% Specific Matlab functions to use are shown between 'quotes'.


% **********************************************************************
% STEP 1. load the file for one of the participants

% --------------------------------------------------------------------------------------
	
	%%%%%%%%%%%%%%%%%%%%%%%%%%%
    %    ADD YOUR CODE HERE   %
	%%%%%%%%%%%%%%%%%%%%%%%%%%%
    load('EEG_Participant2.mat')
% --------------------------------------------------------------------------------------	

%   The structure of the file is as follows:
%   There are four movements, m1 to m4 (see lab script for list of movements)
%   For each movement, the first dimension is the number of channels,
%   the second dimension is the number of trials (i.e., attempts),
%   and the third dimension is the data sample number.
%   Remember that the EEG signals were recorded at 256 sample/s and that
%   Each trial lasts 3s.  Use 'whos' in the Matlab command line to see
%   the size and type of each variable.  
%   You should have four variables: m1 to m4, each with 4 channels
%   5 trials, and 768 samples.
%   The channels are:  ch1= C3,  ch2= Cz,  ch3= C4,  ch4= ear lobe
%   (see file 'biosemi_64ch.gif' for anatomical locations)

% **********************************************************************
% STEP 2. For EEG data, create a bandpass filter with cuttoffs at 4Hz and 65Hz 
%   Change these values accordingly for the EMG data.
%   Use the matlab function 'butter'. Also, create a bandstop filter at 50Hz.
%   For this assignment we'll use order 6 for all filters.

% --------------------------------------------------------------------------------------
	
	%%%%%%%%%%%%%%%%%%%%%%%%%%%
    %    ADD YOUR CODE HERE   %
	%%%%%%%%%%%%%%%%%%%%%%%%%%%
    
% --------------------------------------------------------------------------------------	
Wn = [4/128 65/128]
[b,a] = butter(3, Wn);
h = fvtool(b,a);

Ws = [49/128 51/128]
[d,c] = butter(3, Ws, 'stop');

% STEP 2b. apply the bandpass and bandstop filters to the data.
%   You will need to use a 'For' loop as shown below
%   and use 'filtfilt' with the filter coefficients you created above.

%   Question: why should you not use 'filter' instead?  Include the answer in your report

% Creating zero-filled matrices to speed up processing later
% the matrices have 4 channels, 5 trials, 768 samples (i.e., 3s of data)
m1f=zeros(4,5,768);  % m1f will be the filtered version of m1, etc.
m2f=zeros(4,5,768);
m3f=zeros(4,5,768);
m4f=zeros(4,5,768);

for ch=1:4 
    for trial=1:5
    % hint:
    % m1f(ch,trial,:)= filtfilt(???);
    % etc.
% --------------------------------------------------------------------------------------
	
	%%%%%%%%%%%%%%%%%%%%%%%%%%%
    %    ADD YOUR CODE HERE   %
	%%%%%%%%%%%%%%%%%%%%%%%%%%%
	
% --------------------------------------------------------------------------------------	
        m1f(ch,trial,:) = filtfilt(b,a,m1(ch,trial,:));
        m2f(ch,trial,:) = filtfilt(b,a,m2(ch,trial,:));
        m3f(ch,trial,:) = filtfilt(b,a,m3(ch,trial,:));
        m4f(ch,trial,:) = filtfilt(b,a,m4(ch,trial,:));
        m1f(ch,trial,:) = filtfilt(d,c,m1f(ch,trial,:));
        m2f(ch,trial,:) = filtfilt(d,c,m2f(ch,trial,:));
        m3f(ch,trial,:) = filtfilt(d,c,m3f(ch,trial,:));
        m4f(ch,trial,:) = filtfilt(d,c,m4f(ch,trial,:));
    end
end


% **********************************************************************
% STEP 3. Ear reference the data (only done on EEG data
%    Subtract the ear lobe data (channel 4) from the other three channels

m1fr=zeros(3,5,768);    % m1fr is the referenced version of m1f, etc.
m2fr=zeros(3,5,768);
m3fr=zeros(3,5,768);
m4fr=zeros(3,5,768);

for ch=1:3
    for trial=1:5
    % hint: 
    % m1fr(ch,trial,:)=m1f(ch,trial,:)-m1f(4,trial,:);
    % etc.
% --------------------------------------------------------------------------------------
	
	%%%%%%%%%%%%%%%%%%%%%%%%%%%
    %    ADD YOUR CODE HERE   %
	%%%%%%%%%%%%%%%%%%%%%%%%%%%
	
% --------------------------------------------------------------------------------------	
        m1fr(ch,trial,:) = m1f(ch,trial,:) - m1f(4,trial,:);
        m2fr(ch,trial,:) = m2f(ch,trial,:) - m2f(4,trial,:);
        m3fr(ch,trial,:) = m3f(ch,trial,:) - m3f(4,trial,:);
        m4fr(ch,trial,:) = m4f(ch,trial,:) - m4f(4,trial,:);
    end
end


% NOTE: In your report include a brief comparison of the performance
% results for all participants without ear reference vs with ear referencing
% and discuss possible effects of referencing on performance.


% **********************************************************************
% STEP 4. Estimate the energy

% hint:
% m1e= the square of m1fr;   % m1e is the energy for m1, etc.
% etc.


% --------------------------------------------------------------------------------------
	
	%%%%%%%%%%%%%%%%%%%%%%%%%%%
    %    ADD YOUR CODE HERE   %
	%%%%%%%%%%%%%%%%%%%%%%%%%%%
	m1e = m1fr.*m1fr;
    m2e = m2fr.*m2fr;
    m3e = m3fr.*m3fr;
    m4e = m4fr.*m4fr;
% --------------------------------------------------------------------------------------	



% **********************************************************************
% STEP 5. Feature extraction: reduce all energy data to just
%   8 energy values per second for each channel and trial.
%   To do this, resample the energy to 1/32 of the original sampling rate
%   A different resampling rate will need to be used for EMG

decm1e=zeros(3,5,24);   %decm1e is the downsampled version of m1e
decm2e=zeros(3,5,24);	% 24 is 768/32
decm3e=zeros(3,5,24);
decm4e=zeros(3,5,24);

for ch=1:3
    for trial=1:5
        % hint:
        % decm1e(ch,trial,:)=decimate(m1e(ch,trial,:),32);
        % etc.
		
% --------------------------------------------------------------------------------------
	
	%%%%%%%%%%%%%%%%%%%%%%%%%%%
    %    ADD YOUR CODE HERE   %
	%%%%%%%%%%%%%%%%%%%%%%%%%%%
	decm1e(ch,trial,:) = decimate(m1e(ch,trial,:),32);
    decm2e(ch,trial,:) = decimate(m2e(ch,trial,:),32);
    decm3e(ch,trial,:) = decimate(m3e(ch,trial,:),32);
    decm4e(ch,trial,:) = decimate(m4e(ch,trial,:),32);
    
% --------------------------------------------------------------------------------------	

    end 
end


%   Create training input and target output arrays for a neural
%   network classifier.  To understand what to do, type
%   'help newff' in the Matlab command line.
%   You will use the first 3 trials for training, 
%   and the other 2 for testing, for  each movement.
%   There are 12 columns in the training sets because we
%   are using the first three trials for each of the 4 movements = 12 trials
%
%   P: training input     T: training target outputs
%   Inputs: 3 channels concatenated as one 72-element input vector
%   P is 72 inputs neurons (rows) x 12 trials (columns)
%   T is 4 neurons (rows) x 12 trials (columns)
%   Each output neuron corresponds to one of the movemet classes

%   IMPORTANT: This code is just for initial help.
%              The code lines below work for some versions of Matlab
%              but not for all.  It is your job to make any necessary corrections

P=zeros(72,12);

P(1:24,1:3)=squeeze(decm1e(1,1:3,:))'; 
P(25:48,1:3)=squeeze(decm1e(2,1:3,:))'; 
P(49:72,1:3)=squeeze(decm1e(3,1:3,:))'; 

P(1:24,4:6)=squeeze(decm2e(1,1:3,:))'; 
P(25:48,4:6)=squeeze(decm2e(2,1:3,:))'; 
P(49:72,4:6)=squeeze(decm2e(3,1:3,:))'; 

P(1:24,7:9)=squeeze(decm3e(1,1:3,:))'; 
P(25:48,7:9)=squeeze(decm3e(2,1:3,:))'; 
P(49:72,7:9)=squeeze(decm3e(3,1:3,:))'; 

P(1:24,10:12)=squeeze(decm4e(1,1:3,:))'; 
P(25:48,10:12)=squeeze(decm4e(2,1:3,:))'; 
P(49:72,10:12)=squeeze(decm4e(3,1:3,:))'; 

%  Target output matrix:
%  First three columns are for movement 1, the next three are mov. 2, etc.
T=zeros(4,12);
for j=1:3
    T(1:4,j)=[1 0 0 0];  % movement 1
    T(1:4,j+3)=[0 1 0 0]; % movement 2
    T(1:4,j+6)=[0 0 1 0]; % movement 3
    T(1:4,j+9)=[0 0 0 1]; % movement 4
end


%   Create the test input array, Pts.
%   This is the array that you will use as input to the classifier 
%   after the neural network has been trained.  The array has 
%   2 trials per movement

% Hint:  for mov1 we have the next three lines
Pts(1:24,1:2)=squeeze(decm1e(1,4:5,:))'; 
Pts(25:48,1:2)=squeeze(decm1e(2,4:5,:))'; 
Pts(49:72,1:2)=squeeze(decm1e(3,4:5,:))'; 

% --------------------------------------------------------------------------------------
	
	%%%%%%%%%%%%%%%%%%%%%%%%%%%
    %    ADD YOUR CODE HERE   %
	%%%%%%%%%%%%%%%%%%%%%%%%%%%
Pts(1:24,3:4)=squeeze(decm2e(1,4:5,:))'; 
Pts(25:48,3:4)=squeeze(decm2e(2,4:5,:))'; 
Pts(49:72,3:4)=squeeze(decm2e(3,4:5,:))'; 

Pts(1:24,5:6)=squeeze(decm3e(1,4:5,:))'; 
Pts(25:48,5:6)=squeeze(decm3e(2,4:5,:))'; 
Pts(49:72,5:6)=squeeze(decm3e(3,4:5,:))'; 

Pts(1:24,7:8)=squeeze(decm4e(1,4:5,:))'; 
Pts(25:48,7:8)=squeeze(decm4e(2,4:5,:))'; 
Pts(49:72,7:8)=squeeze(decm4e(3,4:5,:))'; 
% --------------------------------------------------------------------------------------	



Performance=[0 0 0 0 0]'; % initially set the performance pecentages to zero

for runs=1:5  % run the training + testing + performance assessment 5 times
    % we do this five times for 'cross-validation' purposes. I.e., 
	% we start with a randomly connected neural network every time and
	% do the whole process five times.  You need to obtain the average 
	% performance for the 5 runs to get a perfomance measure that is
	% independent of the initial neural netwok randomization
	
	
    % Create a neural net with 30 hidden neurons
	%net=newff(P,T,30,{'tansig','logsig'},'traingdx');
    net = feedforwardnet(30);
    net.TrainParam.epochs=1000;
    net.TrainParam.goal=0.04;
    net.TrainParam.min_grad=0.00000000000000000000000000001;
    net.TrainParam.show=3000;
    net.divideParam.trainRatio=1.0;
    net.divideParam.valRatio=0.0;
    net.divideParam.testRatio=0.0;
    
    % Train the neural network
    net=train(net,P,T);
    view(net)
    Y=sim(net,P);
    [m,whichMov]=max(Y);  % check that the chosen class for each trial is correct

% **********************************************************************
% STEP 6. Run the test input data through the trained neural network
%   Use 'sim' as shown above, but with Pts instead of P.
%   Also, calculate the neural network's performance based on the
%   movement classes.  Out of the 8 test movements, only a percentge will
%   be classified correctly.  That is the performance value you need.

    
% --------------------------------------------------------------------------------------
	
	%%%%%%%%%%%%%%%%%%%%%%%%%%%
    %    ADD YOUR CODE HERE   %
	%%%%%%%%%%%%%%%%%%%%%%%%%%%
    Target = [1 1 2 2 3 3 4 4];
	L = sim(net, Pts);
    [m1,mv] = max(L);
    counter = 0;
    for i=1:8
        if (mv(i) == Target(i)) 
            counter = counter + 1;
        end
    end
    Performance(runs) = counter/8*100;
% --------------------------------------------------------------------------------------	

    
    
% **********************************************************************
% STEP 7. Send the neural network's output from step 6 to control the
%   aircraft.
%   For each 3s trial, you have an output from the network.
%   The following should happen for each output trial from the network:
%   Network output = mov. 1:  increase the roll angle to the right in 10
%             steps of 5 degrees, totalling 50 degrees per movement trial.
%   Network output = mov. 2:  increase the roll angle to the left in 10
%             steps of 5 degrees, totalling 50 degrees per movement trial.
%   Network output = mov. 3:  increase the pitch angle in 10
%             steps of 5 degrees, totalling 50 degrees per movement trial.
%   Network output = mov. 4:  decrease the pitch angle to the right in 10
%             steps of 5 degrees, totalling 50 degrees per movement trial.
%
%   Hint: examine the files aircraft.m, drawgripen.m, and gripen.mat
%   to find out how to send commands to and animate the aircraft.
%   Run aircraft.m to see how this works.


% --------------------------------------------------------------------------------------
	
	%%%%%%%%%%%%%%%%%%%%%%%%%%%
    %    ADD YOUR CODE HERE   %
	%%%%%%%%%%%%%%%%%%%%%%%%%%%
    degree = 5*pi/180;  % Degree to Radian: 5 => 5*pi/180
    pitch=0.0;          % Initialization
    roll=0.0;
    load gripen;
	for move=1:8        % 8 times trial
        for i=1:10      % 10 steps for 5 degree change
            if(mv(move) == 1)        % Movement 1 - roll to right(Right arm)
                roll = roll + degree;
            elseif(mv(move) == 2)    % Movement 2 - roll to left(Left arm)
                roll = roll - degree;
            elseif(mv(move) == 3)    % Movement 3 - Pitch to up (knee extension)
                pitch = pitch + degree;
            elseif(mv(move) == 4)    % Movement 4 - Pitch to down (Knee flexion)
                pitch = pitch - degree;
            end
                        
            drawgripen(0,0,0,pitch,roll,pi,1.5,C,V,F,[80 10]);
            pause(0.1);
        end
    end
% --------------------------------------------------------------------------------------	

% ***************************************************************************
% NOTE: in your report be sure to report the average percent correct
% recognition performance for each method (EEG_no_ref, EEG_ref, EMG) as well as for each participant.

end

