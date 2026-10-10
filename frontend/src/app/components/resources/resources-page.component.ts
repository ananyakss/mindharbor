import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

export interface CrisisHelpline {
  name: string;
  numbers: { display: string; tel: string }[];
  note?: string;
}

export interface ExternalResource {
  name: string;
  description: string;
  url: string;
}

@Component({
  selector: 'app-resources-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './resources-page.component.html',
  styleUrls: ['./resources-page.component.css']
})
export class ResourcesPageComponent {
  protected readonly crisisNotice =
    'If you or someone around you is in immediate danger or needs urgent mental health support, please contact an appropriate emergency or crisis support service.';

  protected readonly disclaimer =
    'These resources are provided for informational and support purposes. If you or someone else is in immediate danger, contact local emergency services or an appropriate crisis service.';

  protected readonly helplines: CrisisHelpline[] = [
    {
      name: 'Tele-MANAS',
      numbers: [
        { display: '14416', tel: '14416' },
        { display: '1800-89-14416', tel: '18008914416' }
      ]
    },
    {
      name: 'AASRA',
      numbers: [{ display: '022-27546669', tel: '02227546669' }],
      note: '24×7'
    },
    {
      name: 'iCALL (TISS)',
      numbers: [{ display: '022-25521111', tel: '02225521111' }],
      note: 'Mon–Sat, 10 AM–8 PM'
    },
    {
      name: 'Vandrevala Foundation',
      numbers: [{ display: '9999 666 555', tel: '9999666555' }],
      note: '24×7'
    },
    {
      name: 'Manodarpan',
      numbers: [{ display: '8448440632', tel: '8448440632' }]
    },
    {
      name: 'KIRAN',
      numbers: [{ display: '1800-599-0019', tel: '18005990019' }],
      note: 'Mental health rehabilitation helpline'
    },
    {
      name: 'NIMHANS Helpline',
      numbers: [{ display: '080-4611 0007', tel: '08046110007' }],
      note: 'Psychosocial support helpline'
    }
  ];

  protected readonly healthcareOrgs: ExternalResource[] = [
    {
      name: 'NIMHANS',
      description: 'National institute for mental health and neurosciences — care, research, and training.',
      url: 'https://www.nimhans.ac.in/'
    },
    {
      name: 'AIIMS New Delhi',
      description: 'All India Institute of Medical Sciences — comprehensive healthcare and specialist services.',
      url: 'https://www.aiims.edu/'
    },
    {
      name: 'National Mental Health Programme (NMHP)',
      description: 'Government of India programme for accessible community mental health services.',
      url: 'https://www.dghs.mohfw.gov.in/national-mental-health-programme.php'
    },
    {
      name: 'Manipal Hospitals',
      description: 'Multi-specialty hospital network including mental health and wellness support.',
      url: 'https://www.manipalhospitals.com/'
    },
    {
      name: 'YourDOST',
      description: 'Online emotional wellness platform with counseling and peer support.',
      url: 'https://yourdost.com/'
    },
    {
      name: 'The Live Love Laugh Foundation',
      description: 'Mental health awareness, education, and community support initiatives.',
      url: 'https://www.thelivelovelaughfoundation.org/'
    }
  ];

  protected readonly wellnessTools: ExternalResource[] = [
    {
      name: 'Headspace',
      description: 'Guided meditation, mindfulness, and sleep content.',
      url: 'https://www.headspace.com/'
    },
    {
      name: 'Insight Timer',
      description: 'Free meditations, music, and talks from teachers worldwide.',
      url: 'https://insighttimer.com/'
    },
    {
      name: 'SleepWatch',
      description: 'Sleep tracking and insights to support restful routines.',
      url: 'https://www.sleepwatchapp.com/'
    },
    {
      name: 'BetterHelp',
      description: 'Online therapy and licensed counselor matching.',
      url: 'https://www.betterhelp.com/'
    },
    {
      name: 'Calm',
      description: 'Meditation, sleep stories, and relaxation exercises.',
      url: 'https://www.calm.com/'
    },
    {
      name: 'Mindfulness Coach',
      description: 'VA mindfulness exercises and practice tools (mobile app).',
      url: 'https://mobile.va.gov/app/mindfulness-coach'
    }
  ];
}
